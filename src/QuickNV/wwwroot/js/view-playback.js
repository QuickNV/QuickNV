$(document).ready(function () {
    function initNiceScroll() {
        if ($.nicescroll == null) {
            setTimeout(initNiceScroll, 100);
            return;
        }
        $('#tree').niceScroll({
            cursorcolor: "#00A2FF",
            cursoropacitymin: 0.3,
            cursoropacitymax: 0.5,
            cursorwidth: "6px",
            cursorborder: "1px solid #00C7FF",
            cursorborderradius: "3px"
        });
    }
    initNiceScroll();

    var getQueryStringObject = function () {
        var obj = {};
        var queryString = window.location.search;
        if (queryString != null && queryString != "") {
            if (queryString.indexOf('?') == 0)
                queryString = queryString.substring(1);
            var array_1 = queryString.split('&');
            for (var i1 = 0; i1 < array_1.length; i1++) {
                var array_2 = array_1[i1].split('=');
                if (array_2.length < 2)
                    continue;
                var key = array_2[0];
                var value = array_2[1];
                obj[key] = value;
            }
        }
        return obj;
    };
    
    //更改标题
    var queryObject = getQueryStringObject();
    if(queryObject.title){
        document.title = queryObject.title;
    }

    var style = $("<style>", { type: "text/css" }).appendTo("head");
    $('input[type=range]').bind("input", function (e) {
        var el = e.target;
        var min = el.min;
        var max = el.max;
        var value = el.value;
        var percent = (value - min) * 100 / (max - min);
        var backgroundStyle = "linear-gradient(90deg,#24dcf7 0%, #1d76e5 " + percent + "%, #061329 " + percent + "%,#061329 100%)";
        console.log(backgroundStyle);
        style.text('input[type=range]::-webkit-slider-runnable-track{background: ' + backgroundStyle + '}');
    });

    var element = document.getElementById('root');
    do {
        element.classList.add('w-100');
        element.classList.add('h-100');
        element = element.parentElement;
    } while (element != null);

    var viewPanelEl = document.getElementById('viewPanel');
    //定时检查设置iframe的点击事件
    setInterval(function () {
        var iframes = viewPanelEl.getElementsByClassName('ChannelLiveIframe');
        for (var i = 0; i < iframes.length; i++) {
            var iframe = iframes[i];
            if (iframe.contentDocument == null)
                continue;
            if (iframe.contentDocument.onclick != null)
                continue;
            //绑定iframe中文档的点击事件
            iframe.contentDocument.onclick = function (iframe) {
                return function () {
                    iframe.click();
                };
            }(iframe);
        }
    }, 100);
});
