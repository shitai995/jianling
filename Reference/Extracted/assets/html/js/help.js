
var tmplHotwords = tmpl('tmpl-hotwords-list');

$hotwords = $("#game-hotwords p");

$(document).ready(function() {
	var val = "";
	var _html = tmplHotwords(val);
	console.log(_html);
	$hotwords.prepend(_html);

	
}); // $$$ $(document).ready
