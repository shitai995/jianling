

var url_search_g = p_util.getUrl("ad/j_search.g", 60*24);
var url_hotwords_g = p_util.getUrl("ad/j_hotwords.g");
var hasSearchBoxTips = true;
var wordsNum = p_util.getUrlQueryPara('num', 10);

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

var pageNo = 1;

var tmplCategory = tmpl('tmpl-category-list');
var tmplHotwords = tmpl('tmpl-hotwords-list');

$ul = $(".game-category ul");
$hotwords = $("#game-hotwords p");
var searchType = 0; // 0-- common, 1 -- hotword
var $viewmore = $(".get-more");
var $viewmore_a = $(".get-more a");
var $warn = $(".no-search");

function loadJson(jsonObj, stat) {
	
	if(p_util.isPreLoad()) {
		return;
	}
	
	window.g_resBaseUrl = jsonObj.resourceUrl;

	if (jsonObj && jsonObj.value) {
		$.each(jsonObj.value, function(index, val){
			var _html = tmplHotwords(val);
			console.log(_html);
			$hotwords.append(_html);
		});
		

	} // $$$ if

	
	//处理viewmore
	$(".get-more").css('display', 'none');//先保证viewmore显示
	$viewmore_a.detach();

}

$(document).ready(function() {
	
	if(jsonData == '' ) {
		
		$.getJSON(url_hotwords_g, {num: wordsNum}, loadJson);
		
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	
	//点击时取消提示语
	$('input').focus(function(evtO) {
		if (hasSearchBoxTips) { $(this).val(''); hasSearchBoxTips = false; }
	});
	
	$viewmore.click(onGetMore);


	$('form').submit(function(evtO) {
		if (hasSearchBoxTips) return false; //
		pageNo = 1;
		$ul.empty();
		onGetMore();
		
		searchType = 0;
		return false;
	});

	
}); // $$$ $(document).ready

/**
 * 获取更多
 */
function onGetMore() {
	
	//处理viewmore
	$(".get-more").css('display', 'block');//先保证viewmore显示
	$viewmore_a.detach();
	$viewmore.append(g_lc.canvasElem);
	
	$.getJSON(url_search_g, {type: searchType, pageno: pageNo++, keyword: $('#keyword').val()}, function(jsonObj, stat) {
			
			window.g_resBaseUrl = jsonObj.resourceUrl;

			if (jsonObj && jsonObj.value) {
				$.each(jsonObj.value, function(index, val){
					var _html = tmplCategory(val);
					console.log(_html);
					$ul.append(_html);
				});
				
				if(jsonObj.value.length == 0 && pageNo==2) {$warn.css('display', 'block');}
				else {$warn.css('display', 'none');};// 隐藏空数据提示框
			} // $$$ if

			
			//数据处理完毕，开始处理viemore
			$(g_lc.canvasElem).detach();
			if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
			else { $(".get-more").css('display', 'none');}//隐藏viewmore框

		}
	);

	
	
} // $$$ function onGetMore(eventObj) {

function keywordClick(obj) {
	searchType = 1; //hotword search
	console.log($(obj).html());
	$('#keyword').val($(obj).html());
	hasSearchBoxTips = false;
	$('form').submit();
	
	return false;
}