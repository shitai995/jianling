/**
 * 分页工具
 * @author yihongli
 */
function LoadingPageBtn(btn1st, btnPrev, btnNext){
	this.newPageNo = 1;
    this.pageNo = 1;
	this.hasMore = false;
	this.getPageFunc = null;
	
	// 分页按钮
	this.$btn1st = btn1st;
	this.$btnPrev = btnPrev;
	this.$btnNext = btnNext;
	
	// 分页样式
	this.grayCls = "btn-gray";
	this.lightCls = "btn-light";
}

LoadingPageBtn.prototype.onPrev = function(){
	if(this.pageNo == 1) return;
	this.newPageNo = this.pageNo > 1 ? this.pageNo-1 : this.pageNo;
	window.scrollTo(0, 0);
	return this.getPageFunc(this.newPageNo, this.$btnPrev);
}

LoadingPageBtn.prototype.onNext = function(){
	if(!this.hasMore) return;
	this.newPageNo = this.pageNo+1;
	window.scrollTo(0, 0);
	return this.getPageFunc(this.newPageNo, this.$btnNext);
}

LoadingPageBtn.prototype.on1st = function(){
	if(this.pageNo == 1) return;
	this.newPageNo = 1;
	window.scrollTo(0, 0);
	return this.getPageFunc(this.newPageNo, this.$btn1st);
}

LoadingPageBtn.prototype.init = function(iGetPageFunc){
	this.getPageFunc = iGetPageFunc;
	var tool = this;
	// 注册翻页事件
	this.$btnPrev.click(function(){
		tool.onPrev();
	});
	this.$btnNext.click(function(){
		tool.onNext();
	});
	this.$btn1st.click(function(){
		tool.on1st();
	});
}

/**
 * 更新按钮样式
 */
LoadingPageBtn.prototype.updateBtn = function(iPageNo, ihasMore) {
	this.pageNo = iPageNo;
	this.hasMore = ihasMore;
	
	if(iPageNo == 1 && !ihasMore) {
		this.$btnNext.parent().hide();
	}
	else {
		this.$btnNext.parent().show();
	}
	
	if(ihasMore) {
		this.$btnNext.rmCls(this.grayCls);
		this.$btnNext.addCls(this.lightCls);
	}
	else {
		this.$btnNext.addCls(this.grayCls);
		this.$btnNext.rmCls(this.lightCls);
	}
	
	if(this.pageNo > 1) {
		this.$btn1st.rmCls(this.grayCls);
		this.$btn1st.addCls(this.lightCls);	
		this.$btnPrev.rmCls(this.grayCls);
		this.$btnPrev.addCls(this.lightCls);
	}
	else {
		this.$btn1st.addCls(this.grayCls);
		this.$btn1st.rmCls(this.lightCls);			
		this.$btnPrev.addCls(this.grayCls);
		this.$btnPrev.rmCls(this.lightCls);		
	}
}

