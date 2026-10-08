inter_ru = {
	cancel: "Отмена",
	done: "Выбрать",
	months: [
		"Январь",
		"Февраль",
		"Март",
		"Апрель",
		"Май",
		"Июнь",
		"Июль",
		"Август",
		"Сенятбрь",
		"Октябрь",
		"Ноябрь",
		"Декабрь"
	],
	monthsShort: [
		"Янв",
		"Фев",
		"Мар",
		"Апр",
		"Май",
		"Июн",
		"Июл",
		"Авг",
		"Сен",
		"Окт",
		"Ноя",
		"Дек"
	],
	weekdays: [
		"Понедельник",
		"Вторник",
		"Среда",
		"Четверг",
		"Пятница",
		"Суббота",
		"Воскресенье"
	],
	weekdaysShort: ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"],
	weekdaysAbbrev: ["П", "В", "С", "Ч", "П", "С", "В"]
};

var date = new Date();
var nextWeekFrom = new Date(date.setDate(date.getDate()-1));
var nextWeekTo = new Date(date.setDate(nextWeekFrom.getDate() +1));
var minDateTo = new Date(date.setDate(nextWeekFrom.getDate() +1));

var optionsFrom = {
	//format: 'yyyy-mm-dd',
	i18n: inter_ru,	
	defaultDate: $('#from').attr('data-first'), //new Date(nextWeekFrom),
	setDefaultDate: true,
	autoClose: true,	
	onSelect: function (el) {
		const ell = new Date(el);
		const setMM = ell.getDate() + 1;
		const setM = new Date(ell.setDate(setMM));
		setMinTo(setM);
	}
};

var optionsTo = {
	//format: 'yyyy-mm-dd',
	i18n: inter_ru,
	minDate: new Date(minDateTo),
	defaultDate: new Date(nextWeekTo),
	setDefaultDate: true,
	autoClose: true
};

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


function checkUrlParam() {
	var requiredColumns = ["DtStart", "DtEnd", "ArticleTitle", "Name", "Text", "Person", "Place", "Event", "Organization"];
	var isSearch = false;
	var formDataSearch = new FormData();
	for (var i = 0; i < requiredColumns.length; i++) {		
		var column = requiredColumns[i];		
		var value = getUrlParameter(column);
		formDataSearch.append(column, value);
    }		
	for (let [name, value] of formDataSearch) {
		if (value != 'null' && value != undefined && value != '') {
			$('#search [name="' + name + '"]').val(value);
			isSearch = true;
		}
	}	
	if (isSearch) {
		$('[name="search"]').trigger('click');
		$('#add_p_search').click();
	}
}

function initSelectable() {
	$("select").formSelect();
}

function SearchPartialMainComplete() {
	$('#clear_filter').css('display', 'inline-flex');
	toastMess(LocaleJs["Поиск_выполнен"], false);	
}

var hasScroll = true;
function updateHasScroll() {
	hasScroll = true;
}
function initMoreBtn() {
	$('#more_btn').on('click', function () {
		showMore(this);
	});
	$(window).scroll(function () {
		if (hasScroll && $(window).scrollTop() >= $(document).height() - $(window).height() - 400) {
			showMore(null);
		}
	});
}


$(document).ready(function () {
	initMoreBtn();
	add_p_searchInit();
	checkUrlParam();
	initSelectable();
	var $from = $("#from").datepicker(optionsFrom);
	var $to = $("#to").datepicker(optionsTo);
	//$("select").formSelect();	
});

var setMinTo = function (vad) {	
	var instance = M.Datepicker.getInstance($("#to"));
	instance.options.minDate = vad;
	
	if (new Date(instance) < vad) {
		instance.setDate(vad);
		$("#to").val(instance.toString());
	}
};


function showMore() {
	var hiddenElements = $('.search-results .hiddenEl'),
		visibleCount = parseInt($('#visibleCount').val());
	if (hiddenElements != null && hiddenElements.length > 0) {
		for (var i = 0; i < visibleCount; i++) {
			$(hiddenElements[i]).removeClass('hiddenEl');
		}
	}
	else {
		$('#more_btn').hide('100');
		toastMess(LocaleJs["Больше_элементов_нет"], true);
		hasScroll = false;
    }
}

function add_p_searchInit() {
	$('#add_p_search').on('click', function () {
		var el = $('#searchExtra');
		$(el).toggleClass('hide_search');
		var icon = $('#add_p_search i');
		if ($(el).hasClass('hide_search')) {
			$(icon).html('keyboard_arrow_down');
		}
		else {
			$(icon).html('keyboard_arrow_up');
        }
	});
}

function clear_search(clear_filter) {
	$('#search input').val('');
	$('select').val($("select option:first").val());	
	//def date
	$('#from').val($('#from').attr('data-first'));
	$("#from").datepicker(optionsFrom);
	$("#to").datepicker(optionsTo);
	$('#search-results-list').empty();
	$(clear_filter).css('display', 'none');
	$('#more_btn').css('display', 'none');
	$('[name="groupCollected"]').val('true');
	hasScroll = true;
}