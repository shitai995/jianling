

var url_search_g = p_util.getUrl("ad/j_playerSearch.g");

var hasSearchBoxTips = true;


//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

var pageNo = 1;

var tmplCategory = tmpl('tmpl-category-list');
$ul = $(".game-category ul");

var $viewmore = $(".get-more");
var $viewmore_a = $(".get-more a");
var $warn = $(".no-search");


$(document).ready(function() {
	
	
	/**
	 * 获取更多
	 */
	function onGetMore() {
		if(p_util.isPreLoad()) {
			return;
		}
		
		//处理viewmore
		$(".get-more").css('display', 'block');//先保证viewmore显示
		$viewmore_a.detach();
		$viewmore.append(g_lc.canvasElem);
		
		
		$.getJSON(url_search_g, {uin: $('#keyword').val()}, function(jsonObj, stat) {
				
				window.g_resBaseUrl = jsonObj.resourceUrl;

				if (jsonObj && jsonObj.value) {
					$.each(jsonObj.value, function(index, val){
						var _html = tmplCategory(val);
						console.log(_html);
						$ul.append(_html);
					});
					
					if(jsonObj.value.length == 0 && pageNo==1) {$warn.css('display', 'block');}
					else {$warn.css('display', 'none');};// 隐藏空数据提示框
				} // $$$ if

				//数据处理完毕，开始处理viemore
				$(g_lc.canvasElem).detach();
				if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
				else { $(".get-more").css('display', 'none');}//隐藏viewmore框

			}
		);

		
		
	} // $$$ function onGetMore(eventObj) {
	
	
	$('form').submit(function(evtO) {
		if (hasSearchBoxTips) return false; //
		pageNo = 1;
		$ul.empty();
		onGetMore();
		return false;
	});
	
	
	//点击时取消提示语
	$('input').focus(function(evtO) {
		if (hasSearchBoxTips) { $(this).val(''); hasSearchBoxTips = false; }
	});
	
	


	
	
}); // $$$ $(document).ready