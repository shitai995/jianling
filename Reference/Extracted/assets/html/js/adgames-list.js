/**
 * 
 */
var pageId = p_util.getUrlQueryPara('pageId');
console.log(pageId);
var url_rank_g = p_util.getUrl("ad/j_adList.g", 60*8);

var tmplHotGame = $.tmpl('tmpl-hot-game');

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

function loading(bg) {
	bg.append($('<div style="position:absolute; left:50%; top:35%;margin-left:-20px;"></div>').append(g_lc.canvasElem));
}

function loadComplete() {
	if(g_lc.canvasElem.parentNode) {
		g_lc.canvasElem.parentNode.removeChild(g_lc.canvasElem);
	}
}

loading($('body'));


function loadJson(jsonObj, stat) {
	if(p_util.isPreLoad()) {
		return;
	}
	
	window.g_resBaseUrl = jsonObj.resourceUrl;
	
	if(jsonObj.extra.adContent != '') {
		$("#notice").html(jsonObj.extra.adContent);
		
		$(".notice").css('display', 'block');
	}
	else {
		$(".notice").css('display', 'none');
	}
	
	// 热门游戏（评分)
	$hotGameCategoryUl = $.q("#w-hot-game-category ul");
	$.empty($hotGameCategoryUl);
	$.each(jsonObj.value, function(key, val) {
		var _html = tmplHotGame(val);
		console.log(_html);
		$hotGameCategoryUl.appendChild($.docFrag(_html));
	});
	
	loadComplete();
}

$(document).ready(function() {
	
	if(jsonData == '') {
		$.getJSON(url_rank_g, {pageId: pageId}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	
}); // $$$ $(document).ready


