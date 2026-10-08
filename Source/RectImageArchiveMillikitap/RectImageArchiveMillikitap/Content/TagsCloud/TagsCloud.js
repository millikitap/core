var table;
var cat, val;
var element;
$(document).ready(function () {
    table = $('#tagsTable').DataTable({
        dom: 'Alfrtip',
        alphabetSearch: {
            column: 0
        },
        "language": {
            "url": LocaleJs["Файл_перевода_Data_Table"]
        }
    });
    element = $('#contentBooks');
    $('.modalHref').on('click', function () {
        cat = $(this).attr('data-cat'),
            val = $(this).attr('data-val');
        showInfo();
    });
    $("#infoModal").animatedModal({
        modalTarget: 'modal-02',
        animatedIn: 'slideInDown',
        animatedOut: 'fadeOutDown',
        color: '#2c2c2cf0',
        beforeOpen: function () {

        },
        afterOpen: function () {

        },
        beforeClose: function () {
            cat = null;
            val = null;
        },
        afterClose: function () {

        }
    });
});
function showInfo() {
    $(element).empty();
    var url = '/TimesMachine/SearchByTag';
    switch (cat) {
        case "Персоны":
            {
                url += '?Person=' + val;
            } break;
        case "События":
            {
                url += '?Event=' + val;
            } break;
        case "Места":
            {
                url += '?Place=' + val;
            } break;
        case "Организации":
            {
                url += '?Organization=' + val;
            } break;
    }
    //get info modal
    $.ajax({
        type: "GET",
        url: url,
        contentType: "application/json; charset=utf-8",
        data: { a: "testing" },
        dataType: "json",
        success: function (data) {
            if (data != null) {
                var el = '';
                $(data).each(function () {
                    var searchByTextResult = '';
                    if (this.searchAttrRectArea != null && this.searchAttrRectArea.length > 0) {
                        var i = 1;
                        $(this.searchAttrRectArea).each(function () {
                            var data_viewerid_link = '';
                            if (this.Data_Viewerid) {
                                data_viewerid_link = '&data-viewerid=' + this.Data_Viewerid;
                            }
                            searchByTextResult += '<div class="searchTextResult"><span><a target="_blank" href="/TimesMachine/View?BookId=' + this.BookId + '&pageId=' + this.PagesId + data_viewerid_link + '"><img class="prev_search_img" loading="lazy" src="/Books/' + this.HashFolder + '/' + this.Preview + '" alt="' + this.Name + '"> </a>' + '<a target="_blank" href="/TimesMachine/View?BookId=' + this.BookId + '&pageId=' + this.PagesId + data_viewerid_link + '">' + LocaleJs["Фрагмент_с_совпадением"] + ' ' + (this.FinderText != null && this.FinderText != "" ? '(' + this.FinderText + ')' : '') + '</a></a></span></div>';
                            i++;
                        });
                        var header = LocaleJs["Результат_поиска_по_номеру"] + ' (' + this.searchAttrRectArea.length + '): ';
                        searchByTextResult = header + searchByTextResult;
                    }
                    el += '<article class="the-grid"> <div class="the-grid-content"> <div class="headline"> <div class="snip1236"> <h1>' + this.BookName + '</h1> </div> <figure> <a target="_blank" href="/TimesMachine/View?BookId=' + this.BookId + '"> <img loading="lazy" src="/Books/' + this.HashFolder + '/' + this.FileNamePreview + '" alt="' + this.Name + '"> </a> </figure> </div>' + searchByTextResult + '</div> </article>'
                });
                if (el != '') {
                    $(element).html(el);
                    $('#infoModal').click();
                }
            }
        },
        error: function () {
            alert('Нет данных по запросу');
        }
    });
}