function bookmark(vieweridSelected) {
    return promise = $.ajax({
        url: "/TimesMachine/isLike",
        contentType: "application/json",
        data: JSON.stringify({ 'viewerid': vieweridSelected }),
        dataType: "json",
        method: "POST",
        async: true
    });
}