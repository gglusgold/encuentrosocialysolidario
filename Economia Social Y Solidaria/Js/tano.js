(function ($) {
    $.redirect = function (target, values, method, pestana) {
        console.log("PP");
        method = (method && method.toUpperCase() == 'GET') ? 'GET' : 'POST';
        if (!values) {
            var obj = $.parse_url(target);
            target = obj.url;
            values = obj.params;
        }
        var form = $('<form>').attr({
            method: method,
            action: target,
        });

        if (pestana != undefined)
            form.attr('target', pestana);

        iterateValues(values, [], form);
        $('body').append(form);
        form.submit();
    };
    //Private Functions
    $.parse_url = function (url) {
        if (url.indexOf('?') == -1)
            return { url: url, params: {} }
        var parts = url.split('?'),
                url = parts[0],
                query_string = parts[1],
                elems = query_string.split('&'),
                obj = {};
        for (var i in elems) {
            var pair = elems[i].split('=');
            obj[pair[0]] = pair[1];
        }
        return { url: url, params: obj };
    }
    var getInput = function (name, value, parent) {
        var parentString;
        if (parent.length > 0) {
            parentString = parent[0];
            for (var i = 1; i < parent.length; ++i) {
                parentString += "[" + parent[i] + "]";
            }
            name = parentString + "[" + name + "]";
        }
        return $("<input>").attr({
            type: "hidden",
            name: name,
            value: value
        });
    };
    var iterateValues = function (values, parent, form) {
        var iterateParent = [];
        for (var i in values) {
            if (typeof values[i] == "object") {
                iterateParent = parent.slice();
                iterateParent.push(i);
                iterateValues(values[i], iterateParent, form);
            } else {
                getInput(i, values[i], parent).appendTo(form);
            }
        }
    };

    window.confirm = function (mensaje, callback, titulo, valueBoton, callbackno, tercerBoton) {
        titulo = titulo || 'Confirmación';
        valueBoton = valueBoton || 'Aceptar';
        tercerBoton = tercerBoton || '';

        var modal = $("<div/>").addClass("modal fade");
        modal.attr("tabindex", "-1");
        var dialog = $("<div/>").addClass("modal-dialog ");
        var content = $("<div/>").addClass("modal-content");

        var header = $("<div/>").addClass("modal-header").append("<h4 class='modal-title'>" + titulo + "</h4>");
        var body = $("<div/>").addClass("modal-body");
        var footer = $("<div/>").addClass("modal-footer");

        var pbody = $("<p/>");
        if (isHTML(mensaje)) {
            body.html(mensaje);
        } else {
            pbody.text(mensaje);
            pbody.appendTo(body);
        }



        var btnAceptar = $("<button type='button' class='btn btn-primary' data-dismiss='modal'>" + valueBoton + "</button>");
        btnAceptar.click(function () {
            if (callback !== undefined)
                callback();
        });

        var btnCancelar = $("<button type='button' class='btn btn-danger' data-dismiss='modal'>No</button>");
        btnCancelar.click(function () {
            if (callbackno !== undefined)
                callbackno();
        });

        footer.append(btnAceptar);
        footer.append(tercerBoton);
        footer.append(btnCancelar);

        content.append(header);
        content.append(body);
        content.append(footer);

        dialog.append(content);
        modal.append(dialog);

        modal.modal({
            keyboard: true,
            backdrop: 'static',
            show: true
        });

    };

    //window.alert = function (mensaje, titulo, callback) {
    //    titulo = titulo || 'Mensaje del sistema';

    //    var modal = $("<div/>").addClass("modal fade");
    //    var dialog = $("<div/>").addClass("modal-dialog");
    //    var content = $("<div/>").addClass("modal-content");

    //    var header = $("<div/>").addClass("modal-header").append("<h4 class='modal-title'>" + titulo + "</h4>");
    //    var body = $("<div/>").addClass("modal-body");
    //    var footer = $("<div/>").addClass("modal-footer");

    //    var pbody = $("<p/>");
    //    if (isHTML(mensaje))
    //        pbody.html(mensaje);
    //    else
    //        pbody.text(mensaje);

    //    pbody.appendTo(body);

    //    var btnAceptar = $("<button type='button' class='btn btn-azul' data-dismiss='modal'>Aceptar</button>");
    //    btnAceptar.click(function () {
    //        if (callback !== undefined)
    //            callback();
    //    });

    //    footer.append(btnAceptar);

    //    content.append(header);
    //    content.append(body);
    //    content.append(footer);

    //    dialog.append(content);
    //    modal.append(dialog);

    //    modal.modal({
    //        keyboard: false,
    //        backdrop: 'static',
    //        show: true
    //    });
    //};
    window.alert = function (mensaje, titulo, callback, botonesExtra) {
        titulo = titulo || 'Mensaje del sistema';


        var modal = $("<div role=\"dialog\"/>").addClass("modal fade");
        var dialog = $("<div/>").addClass("modal-dialog");
        dialog.css({ "overflow-y": "initial !important" });
        var content = $("<div/>").addClass("modal-content");

        var header = $("<div/>").addClass("modal-header").append("");
        header.append("<h4 class='modal-title'>" + titulo + "</h4>");
        var body = $("<div/>").addClass("modal-body");
        //body.css({ "height": "350px", "overflow-y": "auto" });
        var footer = $("<div/>").addClass("modal-footer");

        var pbody = $("<p/>");
        if (isHTML(mensaje))
            pbody.html(mensaje);
        else if (typeof (mensaje) == "object") {
            pbody.append(mensaje);
        } else
            pbody.text(mensaje);

        pbody.appendTo(body);



        if (botonesExtra != undefined) {
            $.each(botonesExtra, function (i, val) {
                var btnExtra = $("<button type='button'/>").addClass("btn");
                btnExtra.text(val.Texto);
                if (val.Cerrar != undefined && val.Cerrar)
                    btnExtra.attr({ "data-dismiss": "modal" });


                if (val.Clase != undefined)
                    btnExtra.addClass(val.Clase);

                btnExtra.click(function (e) {
                    if (val.callback != undefined) {
                        val.callback(e);
                    }
                });

                footer.append(btnExtra);
            })
        } else {
            var btnAceptar = $("<button type='button' class='btn btn-primary' data-dismiss='modal'>Aceptar</button>");
            btnAceptar.click(function () {
                if (callback != null && callback != undefined)
                    callback();
            });
        }

        footer.append(btnAceptar);

        content.append(header);
        content.append(body);
        content.append(footer);

        dialog.append(content);
        modal.append(dialog);

        modal.appendTo("body");

        modal.modal({
            keyboard: false,
            backdrop: 'static',
            show: true,
        });
    };
    function isHTML(str) {
        var a = document.createElement('div');
        a.innerHTML = str;
        for (var c = a.childNodes, i = c.length; i--;) {
            if (c[i].nodeType == 1)
                return true;
        }
        return false;
    }
    ;

    //Menu - Active
    var itemMenuSeleccionado = localStorage.getItem("itemMenu");
    $(".sidebar-nav a[data-menu='" + itemMenuSeleccionado + "']").parent().addClass("active");
    $(".sidebar-nav a[data-menu='" + itemMenuSeleccionado + "']").click();
    $(".sidebar-nav a[data-menu='" + itemMenuSeleccionado + "']").parent().parent().parent().addClass("activeSidebar");

    $(".sidebar-nav a").click(function () {
        var seleccionado = $(this).attr("data-menu");
        localStorage.setItem("itemMenu", seleccionado);
    })

    $("#cerrarSesion").click(function () {
        var seleccionado = $(".sidebar-nav a").first().attr("data-menu");
        localStorage.setItem("itemMenu", seleccionado);
    })

    if ($(".SidebarSubMenu .active").length) {
        $(".SidebarSubMenu .active").parent().show();
    }



    //Submenu
    $(".SidebarSubMenu").click(function () {
        $(this).find("ul").slideToggle(200);
        $("#tablaHoras").bootstrapTable('resetView');
    })

    //Corregir footer al redimensionar ventana
    $(window).on('resize', function () {
        $("table").bootstrapTable('resetView');
    });

    //Tooltip
    $('[data-toggle="tooltip"]').tooltip()


    //Reloj - Inicio
    setInterval(function () {
        var seconds = new Date().getSeconds();
        var sdegree = seconds * 6;
        var srotate = "rotate(" + sdegree + "deg)";

        $("#sec").css({ "-moz-transform": srotate, "-webkit-transform": srotate });

    }, 1000);


    setInterval(function () {
        var hours = new Date().getHours();
        var mins = new Date().getMinutes();
        var hdegree = hours * 30 + (mins / 2);
        var hrotate = "rotate(" + hdegree + "deg)";

        $("#hour").css({ "-moz-transform": hrotate, "-webkit-transform": hrotate });

    }, 1000);


    setInterval(function () {
        var mins = new Date().getMinutes();
        var mdegree = mins * 6;
        var mrotate = "rotate(" + mdegree + "deg)";
        $("#min").css({ "-moz-transform": mrotate, "-webkit-transform": mrotate });

    }, 1000);


    //Reloj - Fin

    //Animacion ACERCA DE
    $(".acercaDeTitulo").animate({ opacity: '1', marginTop: '0px' }, 1000);

    $("#sobreHHHEE .col-md-3").each(function (idx) {
        $(this).delay(idx * 300).animate({ opacity: '1', transform: 'scale(1)' }, 400);

    })


    //$("#sobreHHHEE .col-md-3").animate({ opacity: '0.5' }, 2000);




    Date.prototype.formatPiola = function () {
        return this.getDate() +
                "/" + (this.getMonth() + 1) +
                "/" + this.getFullYear();
    }



    ///*********** LEOOOOOOOOO  ***************//////
    /*
     //COLOR
     function randomColor() {
     var letters = '0123456789ABCDEF'.split('');
     var color = '#';
     for (var i = 0; i < 6; i++) {
     color += letters[Math.floor(Math.random() * 16)];
     }
     
     $("body,#sidebar-wrapper").css({
     background: color
     });
     }
     
     //TEXTO
     function texto() {
     var text = "";
     var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
     
     for (var i = 0; i < 5; i++)
     text += possible.charAt(Math.floor(Math.random() * possible.length));
     
     return text;
     }
     
     
     //AUDIO
     var audioElement = document.createElement('audio');
     var degree = 0;
     $(document).keypress(function (ind) {
     degree = degree + 5;
     if (degree == 50) { degree = degree * -1 }
     $("body").css({ WebkitTransform: 'rotate(' + degree + 'deg)' });
     var random = Math.floor(Math.random() * 6) + 1;
     console.log(ind);
     //escrito +=;
     audioElement.setAttribute('src', 'http://santa.com.ar/leo/demos/typewriter/button-' + random + '.mp3');
     audioElement.play();
     
     $("h2").html(texto());
     randomColor()
     });
     */

})(jQuery);


function tog(v) {
    return v ? 'addClass' : 'removeClass';
}
$(document).on('input', '.clearable', function () {
    $(this)[tog(this.value)]('x');
}).on('mousemove', '.x', function (e) {
    $(this)[tog(this.offsetWidth - 18 < e.clientX - this.getBoundingClientRect().left)]('onX');
}).on('touchstart click', '.onX', function (ev) {
    ev.preventDefault();
    $(this).removeClass('x onX').val('').change();

});






$(function () {

    $("#Perfil").click(function () {

        $.post("/Perfil/GetUsuarioPerfil", function (data) {

            //head
            var ModalFade = $("<div/>").addClass("modal fade").attr({ tabindex: -1, role: "dialog", id: "ModalFade" });
            var ModalDialog = $("<div/>").addClass("modal-dialog").attr("role", "document").css("width", "700px");
            ModalDialog.appendTo(ModalFade);
            var ModalContent = $("<div/>").addClass("modal-content");
            ModalContent.appendTo(ModalDialog);
            var ModalHeader = $("<div/>").addClass("modal-header").attr("id", "ModalHeader");
            ModalHeader.appendTo(ModalContent);
            var buttonClose = $("<button/>").addClass("close")
                    .attr({ type: "button", "data-dismiss": "modal", "aria-label": "Cerrar" })
            var spanTitle = $("<span/>").attr("aria-hidden", "true").html("&times;");
            spanTitle.appendTo(buttonClose);
            buttonClose.appendTo(ModalHeader);
            var LabelTitle = $("<h4/>").addClass("modal-title");
            LabelTitle.html("Mi Perfil");
            LabelTitle.appendTo(ModalHeader);
            //body
            var Modalbody = $("<div/>").addClass("modal-body");
            Modalbody.appendTo(ModalContent);
            var form = $("<form/>").addClass("form-horizontal");
            form.appendTo(Modalbody);

            var Agrupar1 = $("<div/>").addClass("form-group");
            Agrupar1.appendTo(form);

            var labelNombres = $("<label/>").addClass("col-xs-1 col-md-4").text("Nombres");
            labelNombres.appendTo(Agrupar1);
            var DivDivNombresData = $("<div/>").addClass("col-md-1");
            DivDivNombresData.appendTo(Agrupar1);
            var labelNombresData = $("<label/>").css("font-weight", "normal").text(data.Record.nombre)
            labelNombresData.appendTo(Agrupar1);

            var Agrupar2 = $("<div/>").addClass("form-group");
            Agrupar2.appendTo(form);

            var labelApellido = $("<label/>").addClass("col-xs-1 col-md-4").text("Apellido");
            labelApellido.appendTo(Agrupar2);
            var DivDivApellidoData = $("<div/>").addClass("col-md-1");
            DivDivApellidoData.appendTo(Agrupar2);
            var labelApellidoData = $("<label/>").css("font-weight", "normal").text(data.Record.apellido)
            labelApellidoData.appendTo(Agrupar2);

            var Agrupar3 = $("<div/>").addClass("form-group");
            Agrupar3.appendTo(form);

            var labelUsername = $("<label/>").addClass("col-xs-1 col-md-4").text("Nombre de usuario");
            labelUsername.appendTo(Agrupar3);
            var DivDivUsernameData = $("<div/>").addClass("col-md-1");
            DivDivUsernameData.appendTo(Agrupar3);
            var labelUsernameData = $("<label/>").css("font-weight", "normal").text(data.Record.nombreUsuario)
            labelUsernameData.appendTo(Agrupar3);

            var Agrupar5 = $("<div/>").addClass("form-group");
            Agrupar5.appendTo(form);
            var hr = $("<hr/>");
            hr.appendTo(Agrupar5)
            var spanRol = $("<span/>").text("Roles").css("color", "grey");
            spanRol.appendTo(Agrupar5);

            var Agrupar = $("<div/>").addClass("form-group");
            Agrupar.appendTo(form);

            //proyecto
            var labelProyecto = $("<label/>").addClass("col-md-4").text("Proyecto");
            labelProyecto.appendTo(Agrupar);
            var DivDivProyectoData = $("<div/>").addClass("col-md-2");
            DivDivProyectoData.appendTo(Agrupar);
            var proyectoD;
            if (data.Record.Proyectos == true)
                proyectoD = "Si"
            else
                proyectoD = "No"
            var labelProyectoData = $("<label/>").css("font-weight", "normal").text(proyectoD)
            labelProyectoData.appendTo(DivDivProyectoData);
            //mesa
            var labelMesaEntrada = $("<label/>").addClass("col-md-4").text("Mesa de entrada");
            labelMesaEntrada.appendTo(Agrupar);
            var DivDivMesaEntradaData = $("<div/>").addClass("col-md-1");
            DivDivMesaEntradaData.appendTo(Agrupar);
            var esMesaD;
            if (data.Record.esMesa == true)
                esMesaD = "Si"
            else
                esMesaD = "No"
            var labelMesaEntradaData = $("<label/>").css("font-weight", "normal").text(esMesaD)
            labelMesaEntradaData.appendTo(DivDivMesaEntradaData);


            var Agrupar5 = $("<div/>").addClass("form-group");
            Agrupar5.appendTo(form);

            var labelAdminUsuarios = $("<label/>").addClass("col-md-4").text("Admin de usuarios");
            labelAdminUsuarios.appendTo(Agrupar5);
            var DivDivAdminUsuariosData = $("<div/>").addClass("col-md-2");
            DivDivAdminUsuariosData.appendTo(Agrupar5);
            var esAdministradorD;
            if (data.Record.esAdministrador == true)
                esAdministradorD = "Si"
            else
                esAdministradorD = "No"
            var labelAdminUsuariosData = $("<label/>").css("font-weight", "normal").text(esAdministradorD)
            labelAdminUsuariosData.appendTo(DivDivAdminUsuariosData);


            var labelJefatura = $("<label/>").addClass("col-md-4").text("Jefatura de gabinete");
            labelJefatura.appendTo(Agrupar5);
            var DivDivJefaturaData = $("<div/>").addClass("col-md-2");
            DivDivJefaturaData.appendTo(Agrupar5);
            var esJefaturaD;
            if (data.Record.esJefatura == true)
                esJefaturaD = "Si"
            else
                esJefaturaD = "No"
            var labelJefaturaData = $("<label/>").css("font-weight", "normal").text(esJefaturaD)
            labelJefaturaData.appendTo(DivDivJefaturaData);


            var Agrupar7 = $("<div/>").addClass("form-group");
            Agrupar7.appendTo(form);

            var labelaltaBeneficiario = $("<label/>").addClass("col-md-4").text("Alta de beneficiarios");
            labelaltaBeneficiario.appendTo(Agrupar7);
            var DivDivaltaBeneficiarioData = $("<div/>").addClass("col-md-2");
            DivDivaltaBeneficiarioData.appendTo(Agrupar7);
            var altaBeneficiarioD;
            if (data.Record.altaBeneficiario == true)
                altaBeneficiarioD = "Si"
            else
                altaBeneficiarioD = "No"
            var labelaltaBeneficiarioData = $("<label/>").css("font-weight", "normal").text(altaBeneficiarioD)
            labelaltaBeneficiarioData.appendTo(DivDivaltaBeneficiarioData);

            var labelaltaPrograma = $("<label/>").addClass("col-md-4").text("Alta de programas");
            labelaltaPrograma.appendTo(Agrupar7);
            var DivDivaltaProgramaData = $("<div/>").addClass("col-md-2");
            DivDivaltaProgramaData.appendTo(Agrupar7);
            var altaProgramaD;
            if (data.Record.altaPrograma == true)
                altaProgramaD = "Si"
            else
                altaProgramaD = "No"
            var labelaltaProgramaData = $("<label/>").css("font-weight", "normal").text(altaProgramaD)
            labelaltaProgramaData.appendTo(DivDivaltaProgramaData);

            var Agrupar9 = $("<div/>").addClass("form-group");
            Agrupar9.appendTo(form);
            var labelaltaesTesoreria = $("<label/>").addClass("col-md-4").text("Tesorería");
            labelaltaesTesoreria.appendTo(Agrupar9);
            var DivDivaltaesTesoreriaData = $("<div/>").addClass("col-md-2");
            DivDivaltaesTesoreriaData.appendTo(Agrupar9);
            var esTesoreriaD;
            if (data.Record.esTesoreria == true)
                esTesoreriaD = "Si"
            else
                esTesoreriaD = "No"
            var labelaltaesTesoreriaData = $("<label/>").css("font-weight", "normal").text(esTesoreriaD)
            labelaltaesTesoreriaData.appendTo(DivDivaltaesTesoreriaData);

            var labelDGA = $("<label/>").addClass("col-md-4").text("Dga");
            labelDGA.appendTo(Agrupar9);
            var DivDivDGAData = $("<div/>").addClass("col-md-2");
            DivDivDGAData.appendTo(Agrupar9);
            var esDga;
            if (data.Record.esDga == true)
                esDgaD = "Si"
            else
                esDgaD = "No"
            var labelaltaesDgaData = $("<label/>").css("font-weight", "normal").text(esDgaD)
            labelaltaesDgaData.appendTo(DivDivDGAData);


            var Agrupar11 = $("<div/>").addClass("form-group");
            Agrupar11.appendTo(form);
            var hr = $("<hr/>");
            hr.appendTo(Agrupar11)
            var spanNot = $("<span/>").text("Notificaciones").css("color", "grey");
            spanNot.appendTo(Agrupar11);

            var Agrupar10 = $("<div/>").addClass("form-group");
            Agrupar10.appendTo(form);
            var labelEmail = $("<label/>").addClass("col-md-4 control-label").text("Correo");
            labelEmail.appendTo(Agrupar10);
            var DivDivEmail = $("<div/>").addClass("col-md-4");
            DivDivEmail.appendTo(Agrupar10);
            // CHEEK
            var inputEmail = $("<input/>").attr({ type: "checkbox" })
                    .attr("data-on-text", "Si")
                    .attr("data-off-text", "No");

            inputEmail.appendTo(DivDivEmail);
            inputEmail.bootstrapSwitch('state', data.Record.recibeMail);





            //Footer
            var divFooter = $("<div/>");
            divFooter.addClass("modal-footer");
            divFooter.appendTo(ModalContent);

            var divButton = $("<div/>").addClass("btn-toolbar").css("margin-top", "20px");
            divButton.appendTo(divFooter);

            var ButtonGuardar = $("<button/>").attr("type", "button").css("float", "right")
            ButtonGuardar.addClass("btn btn-primary col-xs-2").text("Guardar");
            ButtonGuardar.appendTo(divButton);
            ButtonGuardar.click(function () {
                $.post("/Perfil/setNotificaciones", { recibeMail: inputEmail.bootstrapSwitch('state') }, function (data) {
                    if (data.Record == true) {
                        alert("Las modificaciones sobre su perfil se realizaron correctamente");
                        ModalFade.modal('hide')
                    } else {
                        alert("Las modificaciones sobre su perfil no se realizaron");
                        ModalFade.modal('hide')

                    }
                });
            });

            var ButtonCancelar = $("<button/>").attr("type", "button").attr("data-dismiss", "modal").text("Cancelar").css("float", "right");
            ButtonCancelar.addClass("btn btn-primary col-xs-2").text("Cancelar");
            ButtonCancelar.appendTo(divButton);
            ButtonCancelar.click(function () {

            });

            ModalFade.modal('show')
            ModalFade.on("hidden.bs.modal", function () {
                ModalFade.empty();
            });


        });


    });



    jQuery.fn.addHidden = function (name, value) {
        return this.each(function () {
            var input = $('<input />').attr('type', 'hidden').attr('name', name).attr('value', value)
            $(this).append($(input));
        });
    };

    String.prototype.replaceAll = function (search, replacement) {
        var target = this;
        return target.replace(new RegExp(search, 'g'), replacement);
    };

    String.prototype.capitalizeFirstLetter = function () {
        return this.charAt(0).toUpperCase() + this.slice(1);
    }
});

