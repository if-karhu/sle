$(function () {
 
    $(document).ready(function () {
        var btnSolve = $("<img id=\"solve\" class=\"solveBtn\" src=\"/Content/Icons/solve_disabled.png\" />");
        btnSolve.css({
            left: Math.floor(window.innerWidth / 2) + "px",
            top: Math.floor(window.innerHeight / 2) + "px"
        });
        btnSolve.prop('disabled', true);
        $(document.body).append(btnSolve);
    });

    $(document).on("click",".solveBtn",function ()  {
        var equationsAr = [];
        $("#equations").children().each(function () {equationsAr.push($(this).children(".display").text()) });
        
        $.ajax({
            url: '/Sle/SolveAjax',
            type: 'POST',
            data: {equations: equationsAr},
            dataType: 'html',
            traditional:true 
        })
       .success(function (result) {

           $("#output").empty();
           
           $("#output").append("<math xmlns=\"http://www.w3.org/1998/Math/MathML\"><mo>&#123;</mo>   <!--FUCK-->  <mtable>"
               + result + "</mtable></math>");
          // alert($("#output").html());
          // MathJax.Hub.Typeset();
           MathJax.Hub.Queue(["Typeset", MathJax.Hub, $("#output")[0]]);

        //   var math = MathJax.Hub.getAllJax("output")[0];
          // MathJax.Hub.Queue(["Text", math,result]);
       })
       .error(function (jqXHR, textStatus, errorThrown) {
           alert("jqXHR: " + jqXHR.status + "\ntextStatus: " + textStatus + "\nerrorThrown: " + errorThrown);
       })
    });



    $(document).on('click',".append",function () {
        var newLi = $("<li></li>");
        newLi.append("<span class=\"display\" ></span>");
        newLi.append("<input type=\"text\" class=\"edit\" />");
        $("#equations").append(newLi);
       newLi.children(".edit").focus();
    });

    $(document).on('click', ".display", function () {      
        $(this).parent().children("img").remove();
        $(this).hide().siblings(".edit").show().val($(this).text()).focus();      
    });

    $(document).on('contextmenu', '.display',function (e) {
        $(this).parent().remove();       
        return false;
    })

    $(document).on('mouseover','.info',function () {
        $("#output").empty();
        var li = $(this).parent();
        var dialog = $("<div id=\"dialog\" class=\"dialog\"></div>");
        dialog.append("<img src=\"/Content/Icons/err.png\"/>")
        var pos = li.offset();
        var width = li.width();
        dialog.css({
            left: pos.left + $(this).siblings(".display").width() + 70 + "px",
            top: pos.top - 20 + "px"
        });
        dialog.append($(this).parent().attr("info"));
        $(document.body).append(dialog);
    });

    $(document).on('mouseout', '.info', function () {
        $(".dialog").remove();
    });

    $(document).on('focusout', ".edit", function () {
        // alert("out " +$(this).val());
        if ($(this).val() === "") {
            var root = $(this).parent().parent();          
                $(this).parent().remove();
                return;
        }
        var input = $(this).val();
        var li = $(this).parent();
        $(this).hide().siblings(".display").show().text($(this).val());
        $(this).hide().siblings("img").remove();
        li.append("<img src=/Content/Icons/progress.GIF>");

        $("#solve").prop('disabled', true);
        $('#solve').attr('src', '/Content/Icons/solve_disabled.png');

        $.ajax({
            url: '/Sle/Parse',
            contentType: 'application/html; charset=utf-8',
            type: 'GET',
            data: {parseme: input},
            dataType: 'html'
        })
        .success(function (result) {
            li.children("img").remove();
            var res = result.split('|');
            if (res[1]) {
                li.prepend(res[0]); //no img
                li.append(res[1]); //info img
                li.attr("info", res[2]);
            } else {
                li.prepend(res[0]);
                if ($(".info").length === 0) {
                    $("#solve").prop('disabled', false);
                    $('#solve').attr('src', '/Content/Icons/solve.png');

                }


            }

           
        })
        .error(function (xhr, status) {
            alert(status);
        })
    });

    $(document).on("keypress",'.edit',function (e) {
        if (e.which == '13') {
            e.preventDefault();
            $(this).hide().siblings(".display").show().text($(this).val());

            var nextLi = $(this).parent().next();
            if (nextLi.size() === 0 && $(this).val() !== "") {
                var clone = $(this).parent().clone(true);
                clone.children(".edit").val("");
                clone.children(".display").text("");
                clone.children("img").remove();
                $(this).parent().parent().append(clone);
                nextLi = $(this).parent().next();
            }
            var nextText = nextLi.children(".edit").first();
            var nextDisplay = nextLi.children(".display").first();
            nextDisplay.hide();
            nextLi.children(".edit").show().val(nextDisplay.text()).focus();
        }
    });

});