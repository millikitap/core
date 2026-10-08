var layerIsVisible = true;
var countClick = 0;
var runtimeViewer = null;
function resetCountClick() {
    countClick = 0;
}
function checkFullscreen() {
    if (slideshow.isFullscreen) {
        $('.map canvas').show();
    } else {
        $('.map canvas').hide()
    }
}
var getUrlParameter = function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;
    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');
        if (sParameterName[0] === sParam) {
            return typeof sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
    return null;
};
function navigateToEditor(book, page) {
    $('#slideshow').animate({ opacity: 0 }, { queue: false });
    var url = '/TimesMachine/edit?bookid=' + book + '&pageid=' + page;
    $('.loader').css('display', 'flex');
    window.location.href = url;
}
function ctrlButtonInit() {    
    $('#layersCtrl').on('click', function () {
        if (layerIsVisible) {  
            $(this).html('layers_clear');            
            layerIsVisible = false;
            $('.map canvas').hide();
            $('.openseadragon-canvas svg').hide();
        }
        else {
            $(this).html('layers');            
            layerIsVisible = true;
            $('.map canvas').show();
            resizeView();
            $('.openseadragon-canvas svg').show();
        }
    });
    $('#closeSlidingCtrl').on('click', function () {
        slidingPanel(false);
    });    
}

function stepBtnInit() {
    $('.stepBtn').on('click', function () {
        var data = $(this).attr('data-pos');
        if (data == '-') {
            slideshow.dd.setStep(slideshow.current);
        } else if (data == '+') {
            slideshow.dd.setStep(slideshow.current + 2);
        }
    });
}

//editable page
$.fn.extend({
    editable: function () {
        $(this).each(function () {
            var $el = $(this),
                $edittextbox = $('<input type="text"></input>').css('width', '74px').css('text-align', 'center'),
                submitChanges = function () {
                    if ($edittextbox.val() !== '') {
                        $el.html($edittextbox.val());
                        $el.show();
                        $el.trigger('editsubmit', [$el.html()]);
                        $(document).unbind('click', submitChanges);
                        $edittextbox.detach();
                    }
                },
                tempVal;
            $edittextbox.click(function (event) {
                event.stopPropagation();
            });

            $el.dblclick(function (e) {
                tempVal = $el.html();
                $edittextbox.val(tempVal).insertBefore(this)
                    .bind('keypress', function (e) {
                        var code = (e.keyCode ? e.keyCode : e.which);
                        if (code == 13) {
                            submitChanges();
                        }
                    }).select();
                $el.hide();
                $(document).click(submitChanges);
            });
        });
        return this;
    }
});

function initPageSelect() {
    $('#pageNumber').editable().on('editsubmit', function (event, val) {
        if (!isRtl) {
            slideshow.dd.setStep(val);
        }
        else {
            slideshow.dd.setStep(slideshow.slides.length - val +1);
        }
    });
}

var Lazy, resizeInterval;
async function initLazyImg() {
    var images = $('.lazy');
    Lazy = await new LazyLoad(images, {
        root: null,
        rootMargin: "100px",
        threshold: 0
    });
    //resizeInterval = setTimeout(function () {      
    //    resizeView();
    //}, 1500);
}

function resizeView() {    
    $(window).trigger('resize');    
}

function bookInfoEventsInit () {    
    var closeFormBook = $('#closeBookForm');
    var showFormBook = $('#bookInfoBtn');
    var formInfo = $('#bookInfoForm');
    $(showFormBook).on('click', function () {
        $(formInfo).show();
    });
    $(closeFormBook).on('click', function () {
        $(formInfo).hide();
    });
}

function copySelectInput(input) {
    $(input).select();
    navigator.clipboard.writeText($(input).val());    
}

function modalShareInit() {
    var elements = $('#share.modal-overlay,#share>.modal');
    $('#shareInViewCtrl').click(function () {
        elements.addClass('active');
    });
    $('.close-modal').click(function () {
        elements.removeClass('active');
    });   
}

function collectionsImagesUpdates() {
    var elements = $('.swiper-wrapper .sl-card-wrapper');
    if (elements.length > 0) {
        $('.swiper-wrapper .sl-card-wrapper').each(function () {
            var src = $(this).attr('data-src');
            if (src != null && src != undefined) {
                $(this).css('background-image', 'url(' + src + ')');
            }
        });
    }
}

function modalCollectionInit() {
    var elements = $('#collection.modal-overlay,#collection>.modal');
    $('#collectionViewCtrl').click(function () {        
        collectionsImagesUpdates();
        elements.addClass('active');
    });
    $('.close-modal').click(function () {
        elements.removeClass('active');
    });
}

function modalUsersOnlineShow() {
    var elements = $('#usersOnlineTable.modal-overlay,#usersOnlineTable>.modal');
    elements.addClass('active');
    $('.close-modal').click(function () {
        elements.removeClass('active');
    });
}

function shareMainInit() {
    var imgCurrent = $('.slide.current').find('img:first');
    if (imgCurrent != null && imgCurrent.length > 0) {
        var pageId = $(imgCurrent).attr('data-pgid');
        updateUrlParamInView(pageId);
    }
    //image
    var imgPreview = null;
    var shortSrc = $('.slide.current').find('img:first').attr('src');
    if (shortSrc != null && shortSrc != undefined) {
        imgPreview = location.origin + '//' + shortSrc;
    }
    else {
        shortSrc = null;
    }
    var mainShare = document.getElementById('modalShare');
    var share = Ya.share2(mainShare, {
        content: {
            url: location.href,
            image: imgPreview
        }        
    });
    $('#modalLinkText').val(location.href);
    $('#directLinkText').val(shortSrc != null ? window.location.origin + shortSrc : '-');
    generateQRCode(location.href, '#qrCode');
}

function initAccordion() {
    $(".accordion__title").on("click", function (e) {
        e.preventDefault();
        var $this = $(this);
        if (!$this.hasClass("accordion-active")) {
            $(".accordion__content").slideUp(400);
            $(".accordion__title").removeClass("accordion-active");
            $('.accordion__arrow').removeClass('accordion__rotate');
        }
        $this.toggleClass("accordion-active");
        $this.next().slideToggle();
        $('.accordion__arrow', this).toggleClass('accordion__rotate');
    });
}

function changeCollectionSelect() {
    var btn = $('#changeCollectionSelect');
    if (btn != null && btn.length>0) {
        $(btn).on('change', function () {
            var data_id = $("#changeCollectionSelect option:selected").attr('data-id');            
            Loader4();
            window.location.href = '/TimesMachine/View?BookId=' + data_id;
        });
    }
}

function btnChatWindowInit() {
    if (userIsAuth) {
        $('.usersOnline').on('click', function () {
            modalUsersOnlineShow();
        });
    }
}

$(document).ready(function () {
    btnChatWindowInit();
    changeCollectionSelect();
    modalShareInit(); 
    bookInfoEventsInit();
    initLazyImg();
    initPageSelect();
    initAccordion();
    var loc = window.location;    
    currentURL = loc.protocol + '//' + loc.host;
    fullscreenInit();
    isHelperShow();
    likeInitEvent();

    $('#pageNumber').html('1');
    $('#allPages').html(slideshow.slides.length);    
    
    toPageNav();
    viewAreaEventClick();

    ctrlButtonInit();
    stepBtnInit();  

    bindScrollChatWnd();
    //setInterval(checkFullscreen, 10);
});
function viewAreaEventClick() {
    $('.viewAreaEl').on('click', function () {
        //if (slideshow.isFullscreen) {};
        var viewerid = $(this).attr('data-viewerid');
        var pagesid = $(this).attr('data-pagesid');
        if (viewerid == null || pagesid == null) {
            return;
        }
        getContent(viewerid, pagesid);        
    });
};
function contentIndicator(b) {
    var el = $('.panel_slide-wrap .panel_slide');
    if (el != null && el != undefined) {
        if (b) {
            $(el).addClass('isLoading');
        }
        else {
            $(el).removeClass('isLoading');
        }
    }
}


function saveToStorage(key, val) {
    localStorage.setItem(key, val);
}

function getFromStorage(key) {
    return localStorage.getItem(key);
}

function isHelperShow() {
    var isShow = getFromStorage('help');
    if (isShow == undefined || isShow == null) {
        $('#overlay').show('100');
        helperOk();
    }
    else {
        closeOverlay();
    }
}

function helperOk() {
    saveToStorage('help', 'isShow');
}

function isLikeChange(f) {
    var like = $('#likeCtrl');
    if (f) {
        $(like).addClass('like');
    }
    else {
        $(like).removeClass('like');        
    }
}

//main in slide panel
var sharePanelSlide = null;
function socialLink(pageId) {
    var bookId = parseInt(getUrlParameter('BookId'));
    window.history.replaceState(null, null, "?BookId=" + bookId + "&pageId=" + pageId + "&data-viewerid=" + vieweridSelected);
    try {
        if (sharePanelSlide == null) {
            sharePanelSlide = Ya.share2('linkSocFragment', {
                content: {
                    url: location.href
                }
            });
        }
        else {
            sharePanelSlide.updateContent({                
                url: location.href
            });
        }
    } catch (ex) {
        console.log(ex);
    }
}

//in view change on slide
function updateUrlParamInView(pageId) {
    var bookId = getUrlParameter('BookId');
    vieweridSelected = vieweridSelected != null ? vieweridSelected : 0;    
    window.history.replaceState(null, null, "?BookId=" + bookId + "&pageId=" + pageId + "&data-viewerid=" + vieweridSelected);    
}

var vieweridSelected = 0;
function getContent(viewerid, pagesid) {
    if (viewerid == null || pagesid == null) {
        return;
    }
    vieweridSelected = parseInt(viewerid);
    socialLink(pagesid);
    $('#slidingContent').empty();
    contentIndicator(true);    
    isLikeChange(false); 
    $.ajax({
        url: "/TimesMachine/getContent",
        contentType: "application/json",
        data: JSON.stringify({ 'viewerid': viewerid, "pagesid": pagesid }),
        dataType: "json",
        method: "POST",
        async: true,
        success: function (data, textStatus, xhr) {
            if (data != null) {
                $('#slidingContent').html(data.descriptionHtml);
                var articleTitle = data.otherAttributes.ArticleTitle;
                var titleHeaderTitle = '';
                if (articleTitle != '' && articleTitle != null) {
                    titleHeaderTitle = '<h3>' + articleTitle + '</h3><hr/>';
                    $('#slidingContent').prepend(titleHeaderTitle);
                }                
                //other info
                var person = data.otherAttributes.Person;
                var event = data.otherAttributes.Event;
                var Place = data.otherAttributes.Place;
                var Organization = data.otherAttributes.Organization;                
                //add to header
                var customInfo = '';
                var customInfo =
                    '<div class="customInfoContent">' +
                    '<div class="infoCustomLabel">Отмечено на фрагменте:</div>' +
                    '<div>Персоны: ' + person + '</div>' +
                    '<div>Cобытия: ' + event + '</div>' +
                    '<div>Места: ' + Place + '</div>' +
                    '<div>Организации: ' + Organization + '</div>' +
                    '</div > ';
                $('#slidingContent').append(customInfo);
                isLikeChange(data.isLike);               
            }
            else {
                //$('#closeSlidingCtrl').click();
                toastMess(LocaleJs["Содержимого_нет"], true);
                resetCountClick();
            }
            contentIndicator(false);
        },
        error: function (xhr, textStatus, errorThrown) {
            $('#closeSlidingCtrl').click();            
            alert(LocaleJs["Статья_изменена_или_удалена"]);
            console.log('err:' + textStatus);
            contentIndicator(false);
        }
    });
}

function likeInitEvent() {
    $('#likeCtrl').on('click', function () {
        getLike();
    });
}

function getLike() {
    var promise = bookmark(vieweridSelected);
    promise.success(function (data) {      
        isLikeChange(data.isLike);
        if (data.isLike) {
            toastMess(LocaleJs["Добавлено_в_избранное"], false);
        }
        else {
            toastMess(LocaleJs["Статья_удалена_из_избранного"], false);
        }
    });    
}

var isEventBindClick = false;
function slidingPanel(b) {
    if (b) {
        $('.panel_slide-wrap').css('transform', 'translateX(0)');
    }
    else { $('.panel_slide-wrap').css('transform', 'translateX(100%)'); };
}
var pageId;
function toPageNav() {

    pageId = getUrlParameter('pageId');    
    vieweridSelected = getUrlParameter('data-viewerid');

    if (pageId != null) {
        console.log(pageId);
        var pos = $('[usemap="#img_' + pageId + '"]').attr('data-i');
        console.log(pos);
        if (pos != null && pos != undefined) {            
            closeOverlay();
            if (!isRtl) {
                slideshow.dd.setStep(pos);
            }
            else {
                var rtlPos = pos;
                slideshow.dd.setStep(rtlPos);
            }
        }
        else {            
            var backpos = $('[data-pgId="' + pageId + '"]').attr('data-i');
            slideshow.dd.setStep(backpos);
        }        
    } 
    else {
        if (isRtl) {
            if (slideshow.slides.length > 0) {
                slideshow.dd.setStep(slideshow.slides.length);
            }
        }
        else {
            if (slideshow.slides.length > 0) {
                slideshow.dd.setStep(1);
            }            
        }
    }
}

var mapOptions = {
    zoom: false,
    table: false,
    fill: true,
    fillColor: '696969',
    fillOpacity: 0.3,
    stroke: false,
    strokeColor: '696969',
    strokeOpacity: 1,
    strokeWidth: 1,
    fade: false,
    alwaysOn: true,
    neverOn: false,
    groupBy: false,
    wrapClass: true,
    shadow: false,
    shadowX: 0,
    shadowY: 0,
    shadowRadius: 10,
    shadowColor: '000000',
    shadowOpacity: 0.8,
    shadowPosition: 'outside',
    shadowFrom: false
};

//light search result;
var data_vieweridSearchResult;
function getCurrentDataViewerParam() {
    data_vieweridSearchResult = getUrlParameter('data-viewerid');
}

function initMap() {
    getCurrentDataViewerParam(); 
    $("img[usemap]").mapTrifecta(mapOptions);
    $("img[usemap]").jMap();
    if (!isEventBindClick) {
        $('area').on('click', function () {
            //if (slideshow.isFullscreen) {}
            slidingPanel(true);
            $('#slidingContent').empty();
            $('#slidingContent').html($(this).attr('html'));
        });
        isEventBindClick = true;
    }    
}

function openViewerById() {
    var id = getUrlParameter('data-viewerid');
    if (id != null && id != undefined) {
        $('[data-viewerid="' + id + '"]').click();
        console.log('openViewerById');
    }
}

var currentURL;
function fullscreenInit() {
    $('#fullscreenCtrl').on('click', function () {
        var elContent = $('.panel_slide-wrap');
        if ($(elContent).hasClass('fullContent')) {
            $(elContent).removeClass('fullContent');
        }
        else {
            $('.panel_slide-wrap').addClass('fullContent');
        }
    });
}

(function () {
    var overlay = document.getElementById('overlay'),
        overlayClose = overlay.querySelector('button'),
        header = document.getElementById('header')
        switchBtnn = header.querySelector('button.slider-switch'),
        toggleBtnn = function () {
            if (slideshow.isFullscreen) {
                $('h2').hide('100');
                classie.add(switchBtnn, 'view-maxi');
            }
            else {
                $('h2').show('100');
                classie.remove(switchBtnn, 'view-maxi');                
            }
        },
        toggleCtrls = function () {
            if (!slideshow.isContent) {
                classie.add(header, 'hide');
            }
        },
        toggleCompleteCtrls = function () {
            if (!slideshow.isContent) {
                classie.remove(header, 'hide');                
            }
        },
        slideshow = new DragSlideshow(document.getElementById('slideshow'), {
            onToggle: toggleBtnn,
            onToggleContent: toggleCtrls,
            onToggleContentComplete: toggleCompleteCtrls
        }),        
        toggleSlideshow = function () {
        slideshow.toggle();        
        toggleBtnn();            
        },
        closeOverlay = function () {
            classie.add(overlay, 'hide');
        };
    slideshow.toggle();
    switchBtnn.addEventListener('click', toggleSlideshow);
    overlayClose.addEventListener('click', closeOverlay);    
}());


function scaleXY(x,y) {
    var windowPoint = runtimeViewer.viewport.imageToWindowCoordinates(new OpenSeadragon.Point(x, y))
    var viewportPoint = runtimeViewer.viewport.pointFromPixel(new OpenSeadragon.Point(windowPoint.x, windowPoint.y));   
    return { x: viewportPoint.x, y: viewportPoint.y };
}

function zoomDocument(src) {
    slidingPanel(false);
    var tileSource = {
        type: 'image',
        url: src,
        id: new Date()
    };

    //get Rect map area
    var mapCurrent = $('.slide.current').find('area');
    
    OpenSeadragon.setString("Tooltips.Home", "Центрировать");
    OpenSeadragon.setString("Tooltips.ZoomIn", "Увеличить");
    OpenSeadragon.setString("Tooltips.ZoomOut", "Уменьшить");    

    runtimeViewer = OpenSeadragon({
        id: "zoomDocument",
        defaultZoomLevel: 0,
        showNavigator: false,
        showHomeControl: false,
        showZoomControl: false,
        showFullPageControl: false,
        autoHideControls: true,        
        prefixUrl: 'https://openseadragon.github.io/openseadragon/images/',
        debugMode: false,
        showRotationControl: false
    });
    
    //get Polygon area    
    runtimeViewer.addHandler('open', function () {
        var selectedColor = $('#finderSelectedFragmentColor').val();
        if (mapCurrent != null && mapCurrent.length > 0) {
            //add close button
            
            $(mapCurrent).each(function () {
                var polyArray = [];
                var shape = $(this).attr('shape');
                var viewerid = $(this).attr('data-viewerid');
                var pagesid = $(this).attr('data-pagesid');
                var coords = $(this).attr('originalCoords');
                var object = {};
                object.viewerid = viewerid;
                object.pagesid = pagesid;
                switch (shape) {
                    case 'circle':
                        {
                            var posCoord = coords.split(',');
                            console.log('pos', posCoord);
                            var x = parseFloat(posCoord[0]);                            
                            var y = parseFloat(posCoord[1]);
                            var r = parseFloat(posCoord[2]);

                            var scaleXYRes = scaleXY(x, y);
                            var scaleRRes = scaleXY(r, 0);
                            
                            if (vieweridSelected == parseInt(viewerid)) {
                                var d3Circle = d3.select(overlay.node()).append("circle")
                                    .style("fill", selectedColor)
                                    .attr("r", scaleRRes.x)
                                    .attr("cx", scaleXYRes.x)
                                    .attr("cy", scaleXYRes.y)
                                    .attr("viewerid", viewerid)
                                    .attr("pagesid", pagesid);
                            }
                            else {
                                var d3Circle = d3.select(overlay.node()).append("circle")
                                    .attr("class", 'highlight')
                                    .attr("r", scaleRRes.x)
                                    .attr("cx", scaleXYRes.x)
                                    .attr("cy", scaleXYRes.y)
                                    .attr("viewerid", viewerid)
                                    .attr("pagesid", pagesid);
                            }
                        } break;
                    case 'rect':
                        {
                            var posCoord = coords.split(',');                            
                            console.log('pos',posCoord);
                            var x = parseFloat(posCoord[0]);
                            var y = parseFloat(posCoord[1]);
                            var width = parseFloat(posCoord[2]);
                            var height = parseFloat(posCoord[3]);

                            var scaleXYRes = scaleXY(x, y);
                            var scaleWHRes = scaleXY((width - x), (height-y));

                            if (vieweridSelected == parseInt(viewerid)) {
                                var d3Rect = d3.select(overlay.node()).append("rect")
                                    .attr("x", scaleXYRes.x)
                                    .attr("width", scaleWHRes.x)
                                    .attr("y", scaleXYRes.y)
                                    .style("fill", selectedColor)
                                    .attr("height", scaleWHRes.y)
                                    .attr("viewerid", viewerid)
                                    .attr("pagesid", pagesid);
                            }
                            else {
                                var d3Rect = d3.select(overlay.node()).append("rect")
                                    .attr("x", scaleXYRes.x)
                                    .attr("width", scaleWHRes.x)
                                    .attr("y", scaleXYRes.y)
                                    .attr("class", 'highlight')
                                    .attr("height", scaleWHRes.y)
                                    .attr("viewerid", viewerid)
                                    .attr("pagesid", pagesid);
                            }
                        } break;
                    case 'poly':
                        {
                            var posCoord = coords.split(',');
                            posCoord.forEach(function (item, index) {
                                if (index % 2) {
                                    var elPoly = {};
                                    var loc1 = parseFloat(posCoord[index - 1]);
                                    var loc2 = parseFloat(posCoord[index]);
                                    var XY = scaleXY(loc1, loc2);
                                    elPoly.x = XY.x;
                                    elPoly.y = XY.y;
                                    //console.log('ddd',elPoly);
                                    polyArray.push(elPoly);
                                }
                            });

                            //generate viewport positions
                            if (vieweridSelected == parseInt (viewerid)) {
                                var d3Poly = d3.select(overlay.node()).data([polyArray]).append("polygon")
                                    .attr("points", function (d) {
                                        return d.map(function (d) {
                                            return [d.x, d.y].join(",");
                                        }).join(" ");
                                    })
                                    .attr('class', 'polygon')
                                    .style("fill", selectedColor)
                                    .attr("stroke-width", 0)
                                    .attr("viewerid", viewerid)
                                    .attr("pagesid", pagesid);
                            }
                            else {
                                var d3Poly = d3.select(overlay.node()).data([polyArray]).append("polygon")
                                    .attr("points", function (d) {
                                        return d.map(function (d) {
                                            return [d.x, d.y].join(",");
                                        }).join(" ");
                                    })
                                    .attr('class', 'polygon')
                                    .attr("class", 'highlight')
                                    .attr("stroke-width", 0)
                                    .attr("viewerid", viewerid)
                                    .attr("pagesid", pagesid);
                            }
                        } break;
                }
            });
        }
    });    

    runtimeViewer.gestureSettingsMouse.clickToZoom = false;
    runtimeViewer.open(tileSource);
    $('#zoomDocument').css('visibility', 'visible');

    var overlay = runtimeViewer.svgOverlay();

    runtimeViewer.addHandler('canvas-click', function (event) {
        if (event.quick) {
            //console.log(event);
            var webPoint = event.position;
            var viewportPoint = runtimeViewer.viewport.pointFromPixel(webPoint);
            var imagePoint = runtimeViewer.viewport.viewportToImageCoordinates(viewportPoint);
            console.log(webPoint.toString(), viewportPoint.toString(), imagePoint.toString());
            var clickedEl = event.originalEvent.srcElement;
            console.log(clickedEl);
            var viewerId = $(clickedEl).attr('viewerid');
            var pagesId = $(clickedEl).attr('pagesid');
            //console.log(viewportPoint);        
            if (viewerId != null && pagesId != null) {
                getContent(viewerId, pagesId);
                slidingPanel(true);
            }
        }
        else {
            
        }                
    });  

    

    $(window).resize(function () {
        overlay.resize();
    });

    $('#controlsBtnViewport').css('display','flex')
}

function zoomIn() {
    zoomDocument($('.slide.current').find('img').attr('src'));
}

function hideZoom() {
    slidingPanel(false);
    $('#zoomDocument').empty();
    $('#zoomDocument').css('visibility', 'hidden');
    $('#controlsBtnViewport').hide();
    vieweridSelected = null;
}

function rotateLeft() {
    if (runtimeViewer != null) {
        var curr = runtimeViewer.viewport.getRotation();
        runtimeViewer.viewport.setRotation(curr - 45)
    }
}

function rotateRight() {
    if (runtimeViewer != null) {
        var curr = runtimeViewer.viewport.getRotation();
        runtimeViewer.viewport.setRotation(curr+45)
    }
}

function generateQRCode(val, id) {
    $(id).empty().qrcode({
        width: '200',
        height: '200',
        color: 'black',
        bgColor: 'white',
        text: val
    });
}

function setGetParam(key, value) {
    if (history.pushState) {
        var params = new URLSearchParams(window.location.search);
        params.set(key, value);
        var newUrl = window.location.protocol + "//" + window.location.host + window.location.pathname + '?' + params.toString();
        window.history.pushState({ path: newUrl }, '', newUrl);
    }
}

//Переход к слайду по data-pageid
function viewAttachment(c) {
    var data_pageid = $(c).attr('data-pageid');
    setGetParam('pageId', data_pageid);
    toPageNav();
    toastMess(LocaleJs["Переход_к_странице_по_ссылке_выполнен"], false);
}