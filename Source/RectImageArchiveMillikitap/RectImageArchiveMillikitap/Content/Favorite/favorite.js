$(document).ready(function () {
    $('.favorite_ctrl').on('click', function () {
        isBookmark(this);
    });
});

function isBookmark(el) {    
    var viewerId = $(el).attr('data-viewer');
    var grid = $('#grid_' + viewerId);
    bookmark(viewerId).then(function (data) {
        if (data.isLike) {
            toastMess('Добавлено в избранное', false);
            $(grid).css('opacity', '1');
            $(el).html('delete').attr('title','Удалить из избранного');
        }
        else {
            toastMess('Статья удалена из избранного', false);
            $(grid).css('opacity', '0.4');
            $(el).html('favorite').attr('title', 'Добавить в избранное');
        }
    });    
}