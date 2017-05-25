(function () {
    // your page initialization code here
    // the DOM will be available here




    $("#btn_iniciar").click(function () {
        $("#login").modal("toggle")
    });

    $("#btn_registrarse").click(function () {
        $("#registrarse").modal("toggle")
    });

    $("#btn_perfil").click(function () {
        $.post("/Vecinos/Perfil", function (data) {

            $('#perfil #EditarPerfil').show();
            $('#perfil #GuardarPerfil').hide();
            $('#perfil #helpBlock').hide();


            if (data.Error !== undefined) {
                alert(data.Error, "Error", function () {
                    window.location = "../../Inicio/CerrarSesion";
                });
            }

            var perf = data.Perfil;
            var perfil = $("#perfilform").clone();
            perfil.removeAttr("id")

            perfil.find("#idVecinx").val(perf.idVecinx);
            perfil.find("#emailperfil").attr('disabled', 'disabled').val(perf.correo);
            perfil.find("#nombreperfil").attr('disabled', 'disabled').val(perf.nombre);
            perfil.find("#telefonoperfil").attr('disabled', 'disabled').val(perf.telefono);
            perfil.find("#comunaperfil").attr('disabled', 'disabled').val(perf.comuna);

            //         <div class="form-group">
            //    <label for="mailperfil" class="col-md-4 control-label">Mails al confirmar la compra</label>
            //    <div class="col-md-8">
            //        <input type="checkbox" id="mailperfil" checked data-on-text="Recibir" data-off-text="No" data-off-color="danger" />
            //    </div>
            //</div>
            console.log(perf);
            //type="checkbox" id="mailperfil" checked data-on-text="Recibir" data-off-text="No" data-off-color="danger"
            var alertasDiv = perfil.find("#alertasperfil");
            alertasDiv.empty();
            $.each(perf.alertas, function (i, val) {

                var divNoti = $('<div/>').addClass("form-group");
                var label = $('<label for="" class="col-md-4 control-label"/>').text(val.tipo)
                var divInput = $('<div/>').addClass("col-md-8");
                var input = $('<input />').attr({ id: val.codigo, type: "checkbox", "checked": val.activa, disabled: true })

                divInput.append(input);
                divNoti.append(label);
                divNoti.append(divInput);
                alertasDiv.append(divNoti);
            });

            perfil.show();
            $("#perfil").find(".modal-body").empty().append(perfil);

            var isSwitch;
            isSwitch = $("input[type='checkbox']").data('bootstrap-switch')
            if (!isSwitch) {
                $("input[type='checkbox']").bootstrapSwitch({
                    onText: "Recibir",
                    offText: "No",
                    offColor: "danger"
                });
            }
        });
        $("#perfil").modal("toggle")
    });

    $('#perfil #EditarPerfil').on('click', function (e) {
        $('#perfil #EditarPerfil').hide('slow');
        $('#perfil #helpBlock').show('slow');



        $("#perfil :input[required]").removeAttr('disabled');
        $("#perfil input[type='checkbox']").bootstrapSwitch('disabled', false);
        $('#perfil #GuardarPerfil').show('slow');
    });

    $('#perfil-form').on('submit', function (e) {
        e.preventDefault();
        //$('#perfil').find("button").button('loading')

        var data = $("#perfil form").serialize();
        $.each($("#perfil input[type='checkbox']"), function (i, val) {
            data += "&idsalertas[]=" + $(val).attr("id");
            data += "&valalertas[]=" + $(val).bootstrapSwitch('state');
        });


        console.log(data);
      //  return;

        $.post("/Vecinos/ModificarPerfil", data, function (data) {
            $('#perfil').find("button").button('reset')
            if (data.Error != undefined) {
                alert(data.Error, "Algo salió mal!");
            }
            else if (data.sacar) {
                alert("Al cambiar el mail de ingreso, se deberá confirmar de nuevo la cuenta, revise su correo para volver a ingresar", "Su perfil ha sido cambiado", function () {
                    window.location = "../../Inicio/CerrarSesion";
                });
            }
            else {
                $("#perfil").modal("toggle")
            }
        });
    });



    $('#btn_iniciar').on('click', function (e) {
        $('#btn_registrarse').popover('hide');
    });

    $('#btn_cerrar').on('click', function (e) {
        Cookies.remove("listado");
    });


    $('#btn_registrarse').on('click', function (e) {
        $('#btn_iniciar').popover('hide');
    });

    $("#usuario,#pass").keypress(function () {
        if ($("#divError").is(":visible"))
            $("#divError").hide("slow");
    })

    var error = Cookies.get('Error');
    var info = Cookies.get('Info');
    var mensaje = Cookies.get('Mensaje');
    var mail = Cookies.get('Mail');
    var k = Cookies.get('k');

    if ((error !== undefined && error !== '') || (info !== undefined && info !== '') || (mensaje !== undefined && mensaje !== '') && (mensaje == undefined && mensaje == '')) {
        $("#login").modal("toggle")
        $('#btn_iniciar').popover('show');
        if (error !== "")
            $("#divError").addClass("alert-danger");

        if (info !== "")
            $("#divError").addClass("alert-info");

        if (mensaje !== "")
            $("#divError").addClass("alert-success");

        $("#divError").show("slow");
        $("#sError").text(error || info || mensaje);

        Cookies.remove('Error');
        Cookies.remove('Info');
        Cookies.remove('Mensaje');

        Cookies.remove('Mail');
        Cookies.remove('k');
    }
    else if ((mensaje !== undefined && mensaje !== '') && (mail !== undefined && mail !== '')) {
        $("#cambiar").modal("toggle")
        $("#cambiar").find("#emailolvido").val(mail);
        $("#cambiar").find("#emaila").val(mail);
        $("#cambiar").find("#k").val(k);

        Cookies.remove('Mensaje');
        Cookies.remove('Mail');
        Cookies.remove('k');
    }


    $("#ComprasTanda").click(function (e) {
        $.redirectPost("/Tandas/VerCompras", { admin: true })
    })

    $(document).on("click", "#olvidepass", function (e) {
        $("#olvideform").modal("toggle");
    });

    var password = document.getElementById("password"), confirm_password = document.getElementById("confirmpassword");
    function validatePassword() {
        if (password.value !== confirm_password.value) {
            confirm_password.setCustomValidity("Las contraseñas no coinciden");
        } else {
            confirm_password.setCustomValidity('');
        }
    }

    password.onchange = validatePassword;
    confirm_password.onkeyup = validatePassword;

    var password = document.getElementById("password1"), confirm_password = document.getElementById("confirmpassword1");
    function validatePassword() {
        if (password.value !== confirm_password.value) {
            confirm_password.setCustomValidity("Las contraseñas no coinciden");
        } else {
            confirm_password.setCustomValidity('');
        }
    }

    password.onchange = validatePassword;
    confirm_password.onkeyup = validatePassword;

    $(document).on("submit", "#login-form", function (e) {
        $("#login-form").find("input[type='submit']").button('loading');
        $("#login-form").addHidden('url', window.location.pathname);
        return true;
    });

    $(document).on("submit", "#register-form", function (e) {
        $("#register-form").find("input[type='submit']").button('loading');
        $("#register-form").addHidden('url', window.location.pathname);

        return true;
    });

    $.extend(
        {
            redirectPost: function (location, args) {
                var form = $('<form></form>');
                form.attr("method", "post");
                form.attr("action", location);

                $.each(args, function (key, value) {
                    var field = $('<input></input>');

                    field.attr("type", "hidden");
                    field.attr("name", key);
                    field.attr("value", value);

                    form.append(field);
                });
                $(form).appendTo('body').submit();
            }
        });



})();