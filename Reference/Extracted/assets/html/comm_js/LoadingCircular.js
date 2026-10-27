/**
 * 
 * @authod wangdingwei
 */




/**
 * 
 * @param {Object} heigth
 * @param {Array} rgbColor - [32, 56, 14]
 */
function LoadingCircular(height, rgbColorArray) {

	this.height = height || 16;
	this.rgbColorArray = (null != rgbColorArray && 3 == rgbColorArray.length) ? rgbColorArray : [32, 56, 14]; 
	this.canvasElem = document.createElement("canvas"); //HTMLCanvasElement 
	this.rotation = 0; //旋转次数
	this.intervalId = null; //定时期的id
	this.isRunning = false; //是否正在旋转。
}

/**
 * 绘制一个loading圆
 * 
 * @return HTMLCanvasElement
 */
LoadingCircular.prototype.draw = function() {
	
	if (this.rotation < 0) this.rotation = -this.rotation;
	this.rotation %= 8;
	var h = this.height;
	var canvasElem =  this.canvasElem;
	canvasElem.setAttribute('width', this.height);
	canvasElem.setAttribute('height', this.height);
	
	//CanvasRenderingContext2D interface
	var ctx = canvasElem.getContext("2d");
	
	//弧度，一个周角为2PI
	//每绘制时转的弧度
	var degree = -Math.PI * 2 / 8;
	
	ctx.save();
	
	// clearRect(x,y,width,height) : Clears the specified area and makes it fully transparent
	ctx.clearRect(0, 0, h, h);
	
	// 移到中点
    ctx.translate(h / 2, h / 2);
	
	
	var c = h/10;
	var d = h/5;
	
	 // 把一个圆分成8段,分别绘制
    for (var i = 0; i < 8; i++) {
        ctx.save();
        
		// 旋转一段的度数
        ctx.rotate(i * degree);
        
		// 根据旋转的角度设置颜色与透明度,eg:
        // rotation=0 的时候, alpha的值 1, 0.875 ... 0.125
        // rotation=1 的时候, alpha的值 0.875, 0.75 ... 1
		var alpha = (8 - (i + this.rotation) % 8) / 8;
		//alert(alpha);
        ctx.fillStyle = "rgba(" + this.rgbColorArray[0] + "," + this.rgbColorArray[1] + "," + this.rgbColorArray[2] + "," + alpha + ")";
        // 绘制 fillRect(x,y,width,height) : Draws a filled rectangle
        ctx.fillRect(-c / 2, d, c, d);
        ctx.restore();
    }
    ctx.restore();
    
    this.rotation++;
    this.rotation %= 8;
	
	return canvasElem;
};


/**
 * 绘制一个loading圆
 * 
 * @return HTMLCanvasElement
 */
LoadingCircular.prototype.rotate = function() {
	
	var lc = this;
	this.intervalId = window.setInterval(function(){
			//this.rotation++;
			
			lc.draw();
		}, 125); // 由于分成8段,所以
	
};


/**
 * 停止绘图的定时期
 */
LoadingCircular.prototype.stop = function() {
	window.clearInterval(this.intervalId);
};



/*
var lc = new LoadingCircular();
alert(lc.draw());
*/

