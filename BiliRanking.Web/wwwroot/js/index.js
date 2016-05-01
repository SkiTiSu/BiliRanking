function BuildItem(e, i) {
    var t = MainListTemplate;
    return t = t.replace(/{aid}/g, e.aid),
    t = t.replace(/{play}/g, e.play),
    t = t.replace(/{coins}/g, e.coins),
    t = t.replace(/{video_review}/g, e.video_review),
    t = t.replace(/{favorites}/g, e.favorites),
    t = t.replace(/{review}/g, e.review),
    t = t.replace(/{Fdefen}/g, e.Fdefen),
    t = t.replace(/{author}/g, e.author),
    t = t.replace(/{Fpaiming}/g, e.Fpaiming),
    t = t.replace(/{pic}/g, e.pic),
    t = t.replace(/{title}/g, e.title),
    t = t.replace(/{face}/g, e.face),
    t = t.replace(/{created_at}/g, e.created_at)
}
function LoadMainList(e) {
    for (var i = "", t = e.length, a = 0; t > a; a++) {
        var l = BuildItem(e[a], a + 1);
        i += l
    }
    $("#ListMain").html(i)
}
var MainListTemplate = $("#ListMain").html();
$("#ListMain").html("");
$.get("/api/rank/status", function (result) {
    $("#StatusMain").html(result);
});
$.get("/api/rank/list", function (result) {
    LoadMainList(result);
});

//在此给vc.biliran.moe的开发者orz，非常不要脸的借鉴了代码，可以来打我、骂我……