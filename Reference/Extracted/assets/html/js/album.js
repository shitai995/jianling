var url_album_g = p_util.getUrl("ad/j_album.g", 60*24);
var tmplNewGame = $.tmpl('tmpl-new-game');
var tmplCategory = $.tmpl('tmpl-category-list');

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();
$('body').append($('<div style="position:absolute; left:50%; top:35%;margin-left:-20px;"></div>').append(g_lc.canvasElem));

function loadJson(jsonObj, stat) {
	if(p_util.isPreLoad()) {
		return;
	}
	window.g_resBaseUrl = jsonObj.resourceUrl;
	
	// 新品推荐
	$newGameCategoryUl = $.q("#w-new-game-category ul");
	$.empty($newGameCategoryUl);
	for (i = 0; jsonObj.extra.adGameList && jsonObj.extra.adGameList[i]; i++) {
		var _html = tmplNewGame(jsonObj.extra.adGameList[i]);
		console.log(_html);
		$newGameCategoryUl.appendChild($.docFrag(_html));
	}
	
	// 分类列表
	$ul = $("#category-list ul");
	$.each(jsonObj.value, function(key, val) {
		var _html = tmplCategory(val);
		console.log(_html);
		$ul.append(_html);
	});
	
	$(g_lc.canvasElem).detach();

}

$(document).ready(function() {
	
	if(jsonData == '') {
		$.getJSON(url_album_g, {}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	
}); // $$$ $(document).ready