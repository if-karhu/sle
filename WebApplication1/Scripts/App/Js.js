$(function () {
    
    var CONST = {
        ID_APPENDBUTTON: "#appendButton",
        ID_EQUATIONS: "#equations",
        ID_SOLVEBUTTON: "#solve",
        ID_OUTPUT: "#output",
        ID_CLEARBUTTON: "#clearButton", 
        CLASS_DISPLAY: ".display",
        CLASS_EDIT: ".edit",
        CLASS_INFO: '.info'
    };

    $(document).ready(function () {
        var btnSolve = $("<img id=\"solve\" class=\"solveBtn\" src=\"/Content/Icons/solve_disabled.png\" />");
        var appendButton = $(CONST.ID_APPENDBUTTON);
        btnSolve.css({
            left: Math.floor(appendButton.offset().left + appendButton.width() + 20) + "px",
            top: Math.floor(appendButton.offset().top ) + "px"
        });
        btnSolve.prop('disabled', true);
        $(document.body).append(btnSolve);
    });

    $(document).on("click", CONST.ID_CLEARBUTTON, function () {
        $(CONST.ID_EQUATIONS).empty();
        $(CONST.ID_OUTPUT).empty();
        $(CONST.ID_SOLVEBUTTON).prop('disabled', true);
        $(CONST.ID_SOLVEBUTTON).attr('src', '/Content/Icons/solve_disabled.png');
        $(CONST.ID_EQUATIONS).trigger("resize");
    });

    $(document).on("resize", CONST.ID_EQUATIONS, function () {
        $(CONST.ID_SOLVEBUTTON).css({
            left: Math.floor($(this).offset().left + $(CONST.ID_APPENDBUTTON).width() + 20) + "px",
            top: Math.floor($(this).offset().top + ($(this).height()/2) ) + "px"
        });
    });

    $(document).on("click", CONST.ID_SOLVEBUTTON, function () {
        var equationsAr = [];
        $(CONST.ID_EQUATIONS).children().each(function () {equationsAr.push($(this).children(CONST.CLASS_DISPLAY).text()) });
        
        $.ajax({
            url: '/Sle/SolveAjax',
            type: 'POST',
            data: {equations: equationsAr},
            dataType: 'html',
            traditional:true 
        })
       .success(function (result) {
           var output_solution = result.split("|");

           $(CONST.ID_OUTPUT).empty();
           $(CONST.ID_OUTPUT).append(output_solution[0]);

           $(CONST.ID_OUTPUT).append("<br/>");
           $(CONST.ID_OUTPUT).append("<br/>");
           $(CONST.ID_OUTPUT).append("<br/>");
           $(CONST.ID_OUTPUT).append(output_solution[1]);
          // alert($(CONST.ID_OUTPUT).html());
          // MathJax.Hub.Typeset();
           MathJax.Hub.Queue(["Typeset", MathJax.Hub, $(CONST.ID_OUTPUT)[0]]);

        //   var math = MathJax.Hub.getAllJax("output")[0];
          // MathJax.Hub.Queue(["Text", math,result]);
       })
       .error(function (jqXHR, textStatus, errorThrown) {
           alert("jqXHR: " + jqXHR.status + "\ntextStatus: " + textStatus + "\nerrorThrown: " + errorThrown);
       })
    });



    $(document).on('click',CONST.ID_APPENDBUTTON,function () {
        var newLi = $("<li></li>");
        newLi.append("<span class=\"display\" ></span>");
        newLi.append("<input type=\"text\" class=\"edit\" />");
        $(CONST.ID_EQUATIONS).append(newLi);
        newLi.children(CONST.CLASS_EDIT).focus();
        $(CONST.ID_EQUATIONS).trigger("resize");
    });

    $(document).on('click', CONST.CLASS_DISPLAY, function () {      
        $(this).parent().children("img").remove();
        $(this).hide().siblings(CONST.CLASS_EDIT).show().val($(this).text()).focus();      
    });

    $(document).on('contextmenu', CONST.CLASS_DISPLAY,function (e) {
        $(this).parent().remove();
        if ($(CONST.CLASS_INFO).length === 0 && $(CONST.CLASS_DISPLAY).length !== 0) {
            $(CONST.ID_SOLVEBUTTON).prop('disabled', false);
            $(CONST.ID_SOLVEBUTTON).attr('src', '/Content/Icons/solve.png');
        }
        if ($(CONST.CLASS_DISPLAY).length == 0) {
            $(CONST.ID_SOLVEBUTTON).prop('disabled', true);
            $(CONST.ID_SOLVEBUTTON).attr('src', '/Content/Icons/solve_disabled.png');
        }
        $(CONST.ID_EQUATIONS).trigger("resize");
        return false;
    })

    $(document).on('mouseover',CONST.CLASS_INFO,function () {
        $(CONST.ID_OUTPUT).empty();
        var li = $(this).parent();
        var dialog = $("<div id=\"dialog\" class=\"dialog\"></div>");
        dialog.append("<img src=\"/Content/Icons/err.png\"/>")
        var pos = li.offset();
        var width = li.width();
        dialog.css({
            left: pos.left + $(this).siblings(CONST.CLASS_DISPLAY).width() + 70 + "px",
            top: pos.top - 20 + "px"
        });
        dialog.append($(this).parent().attr("info"));
        $(document.body).append(dialog);
    });

    $(document).on('mouseout', CONST.CLASS_INFO, function () {
        $(".dialog").remove();
    });

    $(document).on('focusout', CONST.CLASS_EDIT, function () {
        // alert("out " +$(this).val());
        if ($(this).val() === "") {
            var root = $(this).parent().parent();          
                $(this).parent().remove();
                return;
        }
        var input = $(this).val();
        var li = $(this).parent();
        $(this).hide().siblings(CONST.CLASS_DISPLAY).show().text($(this).val());
        $(this).hide().siblings("img").remove();
        li.append("<img src=/Content/Icons/progress.GIF>");

        $(CONST.ID_SOLVEBUTTON).prop('disabled', true);
        $(CONST.ID_SOLVEBUTTON).attr('src', '/Content/Icons/solve_disabled.png');

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
                if ($(CONST.CLASS_INFO).length === 0) {
                    $(CONST.ID_SOLVEBUTTON).prop('disabled', false);
                    $(CONST.ID_SOLVEBUTTON).attr('src', '/Content/Icons/solve.png');
                }
            }          
        })
        .error(function (xhr, status) {
            alert(status);
        })
    });

    $(document).on("keypress",CONST.CLASS_EDIT,function (e) {
        if (e.which == '13') {
            e.preventDefault();
            $(this).hide().siblings(CONST.CLASS_DISPLAY).show().text($(this).val());

            var nextLi = $(this).parent().next();
            if (nextLi.size() === 0 && $(this).val() !== "") {
                var clone = $(this).parent().clone(true);
                clone.children(CONST.CLASS_EDIT).val("");
                clone.children(CONST.CLASS_DISPLAY).text("");
                clone.children("img").remove();
                $(this).parent().parent().append(clone);
                nextLi = $(this).parent().next();
                $(CONST.ID_EQUATIONS).trigger("resize");
            }
            var nextText = nextLi.children(CONST.CLASS_EDIT).first();
            var nextDisplay = nextLi.children(CONST.CLASS_DISPLAY).first();
            nextDisplay.hide();
            nextLi.children(CONST.CLASS_EDIT).show().val(nextDisplay.text()).focus();
        }
    });

});