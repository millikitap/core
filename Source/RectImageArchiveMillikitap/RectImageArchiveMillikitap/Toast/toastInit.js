function toastMess(text, isWarning) {
    $.toast({
        text: text,
        loader: true,
        loaderBg: isWarning ? '#bf4d4d' : '#ccc'
    })
}