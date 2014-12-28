$(function () {
    $(".display").click(function () {
        $(this).hide().siblings(".edit").show().val($(this).text()).focus();
    });

    $(".edit").focusout(function () {
        // alert("out " +$(this).val());
        if ($(this).val() === "") {
            var root = $(this).parent().parent();
            if (root.children().size() > 1) {
                $(this).parent().remove();
                return;
            } else {
                $(this).val("ENTER AN EQUATION HERE");
                $(this).attr("f", "true");
            }

        }
        var input = $(this).val();
        var li = $(this).parent();
        $(this).hide().siblings(".display").show().text($(this).val());
        $(this).hide().siblings("img").remove();
        li.append("<img src=/Content/Icons/progress.GIF>");
        $.ajax({
            url: '/Sle/Parse',
            contentType: 'application/html; charset=utf-8',
            type: 'GET',
            data: {parseme: input},
            dataType: 'html'
        })
        .success(function (result) {
            li.children("img").remove();
            li.append(result);
        })
        .error(function (xhr, status) {
            alert(status);
        })


    });

    $('.edit').keypress(function (e) {
        if (e.which == '13') {
            e.preventDefault();
            $(this).hide().siblings(".display").show().text($(this).val());

            var nextLi = $(this).parent().next();
            if (nextLi.size() === 0 && $(this).val() !== "") {
                var clone = $(this).parent().clone(true);
                clone.children(".edit").val("");
                clone.children(".display").text("");
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