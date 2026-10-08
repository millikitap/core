$(document).ready(function () {
	$(".change-password").click(function (e) {
		e.preventDefault();
		$(".password-container").show({});
	});

	$(".change-password").click(function (e) {
		e.preventDefault();
		$(".account-container").hide({});
	});

	$(".back").click(function (e) {
		e.preventDefault();
		$(".account-container").show({});
	});

	$(".back").click(function (e) {
		e.preventDefault();
		$(".password-container").hide({});
	});
});