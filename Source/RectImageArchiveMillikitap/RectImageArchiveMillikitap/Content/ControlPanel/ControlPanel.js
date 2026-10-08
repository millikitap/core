$(document).ready(function () {
    //zoomer
    mediumZoom('.zoom', {
        background: '#0a0a0ae0',
        scrollOffset: 0,
        margin: 50
    });
    //init table            
    $('#all_table,#wait_table,#users_table').DataTable({        
        responsive: true,
        "language": {
            "url": "/Content/DataTable/ru.json"
        },
        "order": [[0, "desc"]]
    });
    //selects
    $('select').select2({  });
    $('select').on('select2:selecting', function (e) {
        //ajax put on server new value
        var selectElname = $(e.currentTarget).attr('id');
        var val = e.params.args.data.text;
        console.log(val);
    });
    //modal
    modalCopyInit();
    initAccordion();
});

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

function applyRole(uId) {
    var rolesIds = $('#roleUser_' + uId).val();
    $.ajax({
        type: "POST",
        url: '/TimesMachine/SetupRoleUser',
        data: { UserId: uId, roles: rolesIds },
        dataType: 'json',
        success: function (data) {
            if (data == 200) {
                alert('Роль успешно установлена.');                
            } else if (data == 401) {
                alert('Ошибка доступа');
            }
        }, error: function () {
            alert('Ошибка при изменении роли');
        }
    });
}

var copyBookId = null;
function modalCopyInit() {
    var elements = $('.modal-overlay, .modal');
    $('.cb').click(function () {
        elements.addClass('active');
        copyBookId = parseInt($(this).attr('data-bookId'));
    });
    $('.close-modal').click(function () {
        elements.removeClass('active');
        copyBookId = null;
    });
}

function copyBook() {
    var copyDir = false;
    var isCheckParam = $('[name="param_copy"]:checked');
    if (isCheckParam.length > 0) {
        var param = $(isCheckParam).attr('value');
        if (param == '1') {
            copyDir = true;
        }
        else {
            copyDir = false;
        }
    }

    var progress = $('#copyProgress'), btn = $('#copyBookBtn');
    $(progress).show();
    $(btn).hide();

    $.ajax({
        type: "POST",
        url: '/TimesMachine/СopyBook',
        data: { bookId: copyBookId, copyDir: copyDir},
        dataType: 'json',
        success: function (data) {
            if (data == 401) {
                alert('Ошибка доступа');
            }
            else if (data == 404) {
                    alert('Книга с таким Id не найдена');
                }
                else if (data == 500) {
                        alert('Ошибка при выполнении операции');
                }
                else {
                        alert('Книга успешно скопирована (Id книги=' + data + '). Выполняется перенаправление на страницу редактирования...');
                        var url = '/TimesMachine/EditorBook?BookId=' + data;
                        window.location.href = url;
                     }

            $(progress).hide();
            $(btn).show();
        }, error: function () {
            alert('Ошибка выполнении операции копирования');

            $(progress).hide();
            $(btn).show();
        }
    });
}