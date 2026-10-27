var isDetailStatus = true;

var resId = p_util.getUrlQueryPara('resId');

console.log(resId);

var url_scoreType_g = p_util.getUrl("ad/j_scoreTypes.g", g_refresh_interval);

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
	
	$(".ranking-title").prepend(jsonObj.extra.game.name);
	
	var tmplScoreType = tmpl('tmpl-score-types');
	$ul = $(".ranking-list");
	$.each(jsonObj.value, function(key, val) {
		var _html = tmplScoreType(val);
		console.log(_html);
		$ul.append(_html);
	});
	
	
	$(g_lc.canvasElem).detach();//全屏detach
	
}

// ready
$(document).ready(function() {
	try{
		if(jsonData == '') {
			$.getJSON(url_scoreType_g, {cpId: cpId, gameId: gameId, sid: sid}, loadJson);
		} else {
			console.log('load json: '+ jsonData);
			loadJson(jsonData);
		}
	}catch(e)
	{
		console.log(e.lineNumber + "[" + e.name +"]" + e.message);
	}
}); // $$$ $(document).ready
