var imgUpload, imgPreview, imgUploadForm, totalFiles, previewTitle, previewTitleText, img;
$(document).ready(function () {
        imgUpload = document.getElementById('upload_imgs')
        , imgPreview = document.getElementById('img_preview')
        , imgUploadForm = document.getElementById('img-upload-form');

    imgUpload.addEventListener('change', previewImgs, false);
    imgUploadForm.addEventListener('submit', function (e) {
        e.preventDefault();
        alert('Начало процесса загрузки файлов');
    }, false);
});
function previewImgs(event) {    
    totalFiles = imgUpload.files.length;

    if (!!totalFiles) {
        $('#uploadBtn').show();
        $(imgPreview).empty();
        imgPreview.classList.remove('quote-imgs-thumbs--hidden');
        previewTitle = document.createElement('p');
        previewTitle.style.fontWeight = 'bold';
        previewTitleText = document.createTextNode(totalFiles + ' выбрано изображений');
        previewTitle.appendChild(previewTitleText);
        imgPreview.appendChild(previewTitle);
    }
    else {
        $('#uploadBtn').hide();
    }

    for (var i = 0; i < totalFiles; i++) {       
        img = document.createElement('img');
        img.src = URL.createObjectURL(event.target.files[i]);        
        //imgPreview.appendChild(img);
        var objectDiv = '<img class="img-preview-thumb" src="' + $(img).attr('src') + '" />';
        $(imgPreview).append(objectDiv);
    }
}