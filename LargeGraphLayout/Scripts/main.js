var canvas, context, slider;
var width = 2000, height = 1600;
var radius = 0.5, lineWidth = 1;
var curr_layer = 0;
var layer_count = 4;

var init = function () {
    canvas = document.getElementById('canvas');
    canvas.width = width;
    canvas.height = height;
    context = canvas.getContext('2d');
    context.lineWidth = lineWidth;
    context.strokeStyle = 'black';
    context.fillStyle = 'blue';
    context.globalAlpha = 0.8;
    $('#node_count_control').ionRangeSlider({
        min: 100,
        max: 1000,
        from: 550,
        onChange: function (data) {
            console.log('onChange');
        },
        onFinish: function (data) {
            console.log('onFinish');
        }
    });
    slider = $('#node_count_control').data('ionRangeSlider');
    $('#left').on('click', function (evt) {
        curr_layer = (curr_layer + 1) % layer_count;
        send_request('api/data/retrieve', {
            DataSet: 'wechat',
            Layer: curr_layer
        });
        $(this).notify("Requesting", {
            autoHide: true,
            className: 'info',
            autoHideDelay: 1000
        });
    });
    $('#right').on('click', function (evt) {
        curr_layer = (curr_layer + layer_count - 1) % layer_count;
        send_request('api/data/retrieve', {
            DataSet: 'wechat',
            Layer: curr_layer
        });
        $(this).notify("Requesting", {
            autoHide: true,
            className: 'info',
            autoHideDelay: 1000
        });
    });
};

var draw_path = function (p1, p2) {
    context.beginPath();
    context.moveTo(p1.x, p1.y);
    context.lineTo(p2.x, p2.y);
    context.stroke();
};

var draw_dot = function (pos) {
    context.beginPath();
    context.arc(pos.x, pos.y, radius, 0, 2 * Math.PI);
    context.fill();
    context.stroke();
};

var send_request = function(path, para) {
    $.post(path, para, function (response) {
        var graph = JSON.parse(response);
        console.log(graph);
        var nodes = graph.Nodes;
        var links = graph.Links;
        nodes = project(nodes, width, height);
        draw_graph(nodes, links);
        $('canvas').notify("painting", {
            autoHide: true,
            className: 'info',
            autoHideDelay: 1000
        });
    });
};

var project = function (nodes, width, height) {
    var max_x = Number.MIN_VALUE, max_y = Number.MIN_VALUE;
    var min_x = Number.MAX_VALUE, min_y = Number.MAX_VALUE;
    for (var i = 0; i < nodes.length; i ++) {
        max_x = Math.max(max_x, nodes[i].x);
        max_y = Math.max(max_y, nodes[i].y);
        min_x = Math.min(min_x, nodes[i].x);
        min_y = Math.min(min_y, nodes[i].y);
    }
    var w = Math.max(1, max_x - min_x);
    var h = Math.max(1, max_y - min_y);
    for (var i = 0; i < nodes.length; i ++) {
        nodes[i].x = width * (nodes[i].x - min_x) / w;
        nodes[i].y = height * (nodes[i].y - min_y) / h;
    }
    return nodes;
};

var draw_graph = function (nodes, links) {
    context.clearRect(0, 0, width, height);
    nodes = project(nodes, width, height);
    for (var i = 0; i < links.length; i ++) {
        var link = links[i];
        draw_path(nodes[link.source], nodes[link.target]);
    }
    for (var i = 0; i < nodes.length; i ++) {
        draw_dot(nodes[i]);
    }
    $('#right').notify(curr_layer + '', {
        autoHide: true,
        className: 'info',
        autoHideDelay: 1000
    });
};

init();
send_request('api/data/retrieve', {
    DataSet: 'wechat',
    Layer: 0
});
