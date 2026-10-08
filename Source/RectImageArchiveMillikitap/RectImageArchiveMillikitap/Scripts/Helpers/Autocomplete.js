$(document).ready(function () {

    var elements = $('[data-suggest]');
    $(elements).each(function () {
        bindAutocomplete(this);
    }); 

    function bindAutocomplete(el) {
        $(el).on('input', function () {            
            var id = $(this).attr('id');
            var type = $(this).attr('data-type');           
            var parent = $(el).parent('.l-bord:first');
            var sugg = $(parent).find('.suggestion');
            var hiddenInput = $(parent).find('[real]').length>0;            

            console.log(id + " : " + type + " : " + parent + " : " + sugg);

            $(sugg).empty();
            $(sugg).hide();

            if ($(this).val().length > 1) {
                $.ajax({
                    type: "POST",
                    url: '/TimesMachine/Autocomplete',
                    data: { 's': $(this).val(), 'type': type },
                    async:true,
                    success: function (data) {
                        console.log(data);
                        $(sugg).empty();
                        $(sugg).hide();

                        if (data.length > 0) {
                            $(data).each(function () {
                                $(sugg).append('<div data-hidden="' + hiddenInput+'" el-id="' + id+'" data-id="' + this.Id + '" data-val="' + this.Value + '" data-type="' + type + '" onclick="setText(this)" class="sugg_val">' + this.Value + '<div>');
                            });
                            $(sugg).show();
                        }
                        else {
                            $(sugg).hide();
                        }
                    }, error: function () {
                        $(sugg).hide();
						toastMess('По вашему запросу ничего не найдено', true);						
					}
				});
            }
        });

        $(el).on('click', function () {
            $('.suggestion').empty();
        });
    }    
});

function changeEventAutocomplete(el, name) {
    var val = $(el).val();
    var id = $(el).attr('value');
    var el = $('[name="' + name + '"]');
    if (val != '') {
        $(el).val(id);
    }
    else {
        $(el).val('');
    }
}

function setText(el) {
    var elId = $(el).attr('el-id'),
        dataHidden = $(el).attr('data-hidden'),
        type = $(el).attr('data-type'),
        val = $(el).attr('data-val'),
        dataId = $(el).attr('data-id');
    var el = $('#' + elId);
    $(el).val(val);
    //find form_val el     
    if (dataId != null && dataId != undefined && dataId != '') {
        $(el).attr('value', dataId);
    }
    $('.suggestion').empty();
    $(el).focus();
    $(el).trigger('change');
}