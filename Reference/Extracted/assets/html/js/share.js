
// csnt
var PLAYER_TYPE_ID_ALL = 0; //热门榜单
var PLAYER_TYPE_ID_FRIEND = 1; //新品榜单
var PLAYER_TYPE_ID_NEAR = 2; //评分榜单
var PLAYER_TYPE_ARRAY = [PLAYER_TYPE_ID_ALL, PLAYER_TYPE_ID_FRIEND, PLAYER_TYPE_ID_NEAR];

var typeId = PLAYER_TYPE_ID_FRIEND;
console.log(typeId);

var url_rank_g = p_util.getUrl("ad/j_RecommendPage.g");
var url_share_g = p_util.getUrl("ad/j_postRecommend.g");


//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

var pageNo = 1;
var loadTitle = false;
var tmplCategory = tmpl('tmpl-player-list');
$ul = $(".player-share-list");

$(document).ready(function() {

	var $viewmore = $("#get-more");
	var $viewmore_a = $("#get-more a");
	

	
	/**
	 * 获取更多游戏
	 */
	function onGetMore() {
		$("#get-more").css('display', 'block');//先保证viewmore显示
		$viewmore_a.detach();
		$viewmore.append(g_lc.canvasElem);
		
		
		
		$.getJSON(url_rank_g, { pageno: pageNo++, typeId: typeId}, function(jsonObj, stat) {
				
				if(p_util.isPreLoad()) {
					return;
				}
				
				window.g_resBaseUrl = jsonObj.resourceUrl;

				if (jsonObj && jsonObj.value) {
					if(!loadTitle)
					{
						loadTitle = true;
						$(".achievement-title").html('分享\''+jsonObj.extra.recommendGame + '\'给好友');
					}
					
					$.each(jsonObj.value, function(index, val){
						var _html = tmplCategory(val);
						console.log(_html);
						$ul.append(_html);
					});
				}


				$(g_lc.canvasElem).detach();
				
				if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
				else { $("#get-more").css('display', 'none');}//隐藏viewmore框
				//*/

			}
		);

		
		
	} // $$$ function onGetMore(eventObj) {

	$('#get-more a').click(onGetMore);
	
	onGetMore();
	
	function onShare() {
		var nickNameList = [];
		var uins = $(".check-box").filter(".selected").map(function(){
			var uin = this.id;
			nickNameList.push(this.name);
			
			$("#li-"+uin).remove();// 去除已经分享过的用户
			return uin;
		}).get().join("|");
		
		console.log(uins);
		
		$.getJSON(url_share_g, {uins: uins}, function(jsonObj, stat) {
			if(uins.length == 0) {
				p_pkg.showMsg('分享列表不能为空');
				return;
			}
			if (jsonObj.result != 0) { //推荐失败
				p_pkg.showMsg(jsonObj.msg);
				return;
			}
			p_pkg.showMsg('已分享给好友：'+nickNameList.join(","));

		} );
	}
	
	$('#share a').click(onShare);
	
	$(document).ajaxError(function() {p_pkg.showMsg('网络异常，请稍后再试！'); });
	
	
}); // $$$ $(document).ready



function onClickBox(box) {
	if($(box).hasClass('selected')) {
		$(box).removeClass('selected');
	}else{
		$(box).addClass('selected');
	}
	
};
