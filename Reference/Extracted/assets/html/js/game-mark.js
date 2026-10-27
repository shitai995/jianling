var url_gameDetail_g = p_util.getUrl("ad/j_gameMark.g", 0.1);
var url_postMark_g = p_util.getUrl("ad/j_postMark.g");
var url_comment_g = p_util.getUrl("ad/j_comment.g");

var scoreInfoList = ['很差', '较差', '还行', '推荐', '力荐'];
var hasScore = false;
var starNo = 0;
var userScore = 0;
var initComment = true;

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();
$('body').append($('<div  style="position:absolute; left:50%; top:35%;margin-left:-20px;"></div>').append(g_lc.canvasElem));

var pagesize = p_util.getUrlQueryPara('pagesize', 15);
console.log(pagesize);

var $viewmore = $("#get-more");
var $viewmore_a = $("#get-more a");
var $star = $('.star');
var $star_div = $('.star div');

var tmplComment = tmpl('tmpl-comment-list');
var $ul = $("#comment-list");
var $warn = $(".nodata-wrapper");
var pageNo = 1;
var $commentForm = $('#comment');
var $makesure = $("#makesure");
var $scoreInfo = $("#scoreInfo");

var enableScore = true;

/**
 * 渲染评分页
 */
function loadJson(jsonObj, stat) {
	if(p_util.isPreLoad()) {
		return;
	}
	
	window.g_resBaseUrl = jsonObj.resourceUrl;
	isUserLogin = jsonObj.isUserLogin;
	userScore = jsonObj.value.userScore;
	starNo = userScore/2;
	hasScore = userScore > 0 ;
	enableScore = jsonObj.value.enableComment;
	console.log("enableScore:"+ enableScore);
	
	$(".game-title").html(tmpl('tmpl-game-title', jsonObj.value));
	$("#mark").html(tmpl('tmpl-mark', jsonObj.value));
	$star_div.css('width', userScore + "0%");
	
	loadComments(jsonObj.value.comments, jsonObj.value.hasMore);
	
//	if(!jsonObj.value.hasPlayed) {
//		$(".mark").css('display', 'none');
//	}
	
	$(g_lc.canvasElem).detach();//全屏detach
}

/**
 * 渲染评论列表
 */
function loadComments(jsonObj, hasMore) {
	if(p_util.isPreLoad()) {
		return;
	}
	
	$viewmore.css('display', 'block');//先保证viewmore显示
	$viewmore_a.detach();
	$viewmore.append(g_lc.canvasElem);
	
	var dataCnt = 0;
	if (jsonObj) {
		var value = jsonObj;//jsonObj.value ? jsonObj.value : jsonObj.comments;
		if(value) {
			if(pageNo == 1) { $ul.html('');  }
			$.each(value, function(index, val){
				if(val.content!='') {
					var _html = tmplComment(val);
					console.log(_html);
					$ul.append(_html);
					dataCnt++;
				}

			});
		}
	}
	
	// 隐藏空数据提示框
	if(dataCnt == 0 && pageNo==1) {
		$warn.css('display', 'block');
	}
	else {
		$warn.css('display', 'none');
	};
	

	pageNo++;
	
	$(g_lc.canvasElem).detach();
	
	if (hasMore > 0) { $viewmore.append($viewmore_a); }
	else { $viewmore.css('display', 'none');}//隐藏viewmore框
	
	
}

/**
 * 获取更多评论
 */
function onGetMore() {
	$.getJSON(url_comment_g, { pageno: pageNo, pagesize: pagesize}, function(jsonObj, stat) {
		loadComments(jsonObj.value, jsonObj.more);
	});
}

// ready
$(document).ready(function() {
	
	if(jsonData == '') {
		$.getJSON(url_gameDetail_g, {cpId: cpId, gameId: gameId, pagesize: pagesize}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	

	$viewmore.click(onGetMore);
	
	//获取星星位置代码
	console.log($star.html());
	
	console.log("enableScore:"+ enableScore);
	
	$commentForm.click(function(evt0) {
		if(initComment) {
			initComment = false;
			$commentForm.val('');
		}
	});
	// $$$ $commentForm.click
	
	//提交评分
	$makesure.click(function(evtO) {
		if(initComment) {
			initComment = false;
			$commentForm.val('');
		}
		
		var comment = $commentForm.val();
		if (starNo <= 0 && comment == '') {
			 alert("评分或评论不允许为空。");
			 return false;
		}

		hasScore = true;
		if (starNo > 5) starNo = 5;
		
		$.getJSON(url_postMark_g, {score: starNo*2, content: comment, pagesize: pagesize}, function(jsonObj, stat) {
			$scoreInfo.html("已评分");
			p_pkg.showMsg(jsonObj.msg+"!");
			$makesure.hide();
			if(jsonObj.result == 0) {
				pageNo = 1;
				onGetMore();
				
				// 发布评论成功，则不允许重复提交
				enableScore = false;
			}
			
		});

		return false;
	});// $$$ $("#makesure").click
	
	if(enableScore) {

		$star.mousemove(function(evtO) {
			if(!enableScore) {return false;}
			if (!isUserLogin) {
				console.log('未登录，请先登录再进行评分或评论。');
				p_util.login();
				return;
			}
			
			$makesure.show();

			var _$star = $(this);
			var _w = _$star.width()/5;//一颗星的宽度
			var _p = evtO.pageX - _$star.offset().left;//相对于div的位置 
			var _starIndex = Math.ceil(_p/_w); //鼠标所在的星
			starNo = _starIndex;

			$star_div.css('width', (_starIndex*2) + "0%");
			var _scoreInfo = (starNo<1 || starNo>5 ? '' : scoreInfoList[starNo-1]);
			$scoreInfo.html(_scoreInfo);
		});
		// $$$ $star.mousemove
		
	}// $$$ if(enable)
	
}); // $$$ $(document).ready

