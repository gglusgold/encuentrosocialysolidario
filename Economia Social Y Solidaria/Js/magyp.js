$.widget("plugin.Magyp", {
    options: {
        nombre: "Nombre del Dialog",
        fields: {},
        ancho: "500px",
        editando: false,
        editandoIndex: null,
        actual: 0,
        lista: [],
        info: null,
        url: null,
        urlEditar: null,
        parametrosGet: null,
        callback: null,
        grabado: null,
        unico: false,
        pasopaso: false,
        ultimo: false,
        ajaxSettings: {
            type: 'POST'
                    // dataType: 'json'
        }
    },
    Mostrar: function (editando, info, index) {

        this.options.editandoIndex = index;

        this.options.editando = editando;
        this.options.info = info;

        this._crearVentana();

        this.element.modal('show');

    },
    //----------------Esto hace primero------------------------------------
    PasoPaso: function (info) {

        this.options.info = info;
        this._crearVentana();

        this.element.modal('show');
        this.options.pasopaso = true;
    },
    //---------------------------------------------------------------------


    //----------------Esto hace segundo lo llama PasoPaso------------------
    _crearVentana: function () {
        var principal = this;

        this.element.empty();
        this.element.addClass("modal fade").attr("data-backdrop", "static");
        this.element.addClass("modal fade").attr("data-keyboard", "false");


        var divDialog = $("<div/>").addClass("modal-dialog");
        divDialog.css("width", this.options.ancho);
        divDialog.appendTo(this.element);

        var DivContent = $("<div/>").addClass("modal-content");
        DivContent.appendTo(divDialog);

        this._crearTitulo();
        this._crearContenido();
        this._crearBotones();

        this._crearFormulario();

        //esto es para los select anidados de tipo (pais, provincia, ciudad)
        $.each(principal.options.fields, function (i, val) {



        });
    },
    //---------------------------------------------------------------------


    //----------------Esto hace tercero lo llama _crearVentana-------------
    _crearTitulo: function () {
        var principal = this;
        var DivContent = this.element.find(".modal-content");

        var div = $("<div/>").addClass("modal-header");
        div.appendTo(DivContent);

        var titulo = this.options.editando ? "Editar " : "";
        if (this.options.unico)
            titulo = "";



        if (!this.options.editando && !this.options.unico) {
            var deshabilitar = this.options.lista.length === 0 || (this.options.actual + 1) >= this.options.lista.length;

            var btnagregar = $("<button/>")
                    .css("float", "right")
                    .css("margin-top", "-30px")
                    .text("Agregar Nuevo " + this.options.nombre).appendTo(div)
                    .attr("disabled", !deshabilitar)
                    .attr("data-agregar", "true")
                    .attr("data-toggle", "popover")
                    .attr("data-trigger", "focus")
                    .attr("title", principal.options.nombre)
                    .attr("data-content", "Haciendo click en el boton, continúa la carga de " + this.options.nombre)
                    .attr("data-placement", "bottom");

            btnagregar.click(function () {
                if (principal.element.find("form").validationEngine('validate')) {
                    principal._grabarEnLista();
                    principal.options.actual++;
                    principal._crearVentana();
                }
            });
        }

        var btncancelar = $("<button/>").addClass("close").html('<span class="glyphicon glyphicon-remove"></span>').appendTo(div);
        $("<h4/>").addClass("modal-title").text(titulo + this.options.nombre).appendTo(div);
        btncancelar.click(function () {
            confirm("Si cierra perderá todos los cambios, ¿desea continuar?", function () {
                principal.options.actual = 0;
                principal.options.lista = [];
                principal.element.modal("hide");
            }, principal.options.nombre);
        });

    },
    _crearContenido: function () {

        var DivContent = this.element.find(".modal-content");

        var div = $("<div/>").addClass("modal-body");
        div.appendTo(DivContent);
    },
    _crearBotones: function () {
        var principal = this;
        var DivContent = this.element.find(".modal-content");

        var div = $("<div/>").addClass("modal-footer");
        div.appendTo(DivContent);

        if (!this.options.editando) {

            var deshabilitar = this.options.actual === 0;
            var btnatras = $("<button/>").addClass("btn btn-default").css("float", "left").html('<span class="glyphicon glyphicon-chevron-left"></span> Anterior').appendTo(div);

            if (deshabilitar)
                btnatras.hide();
            else
                btnatras.show();

            btnatras.click(function () {
                if (principal.element.find("form").validationEngine('validate')) {
                    principal._grabarEnLista();
                    principal.options.actual--;
                    principal._crearVentana();
                }
            });

            deshabilitar = this.options.lista.length === 0 || (this.options.actual + 1) >= this.options.lista.length;
            var btnsiguiente = $("<button/>").addClass("btn btn-default").css("float", "left").html('Posterior <span class="glyphicon glyphicon-chevron-right"></span>').appendTo(div);

            if (deshabilitar)
                btnsiguiente.hide();
            else
                btnsiguiente.show();

            btnsiguiente.click(function () {
                if (principal.element.find("form").validationEngine('validate')) {
                    principal._grabarEnLista();
                    principal.options.actual++;
                    principal._crearVentana();
                }
            });

        }



        var btnBorrar = $("<button/>").addClass("btn btn-warning").html('<span class="glyphicon glyphicon-trash"></span> Borrar actual');

        if (!principal.options.editando && !principal.options.unico)
            btnBorrar.appendTo(div);

        btnBorrar.attr("disabled", this.options.lista.length === 0 || this.options.actual === 0 && this.options.lista.length === 1);
        btnBorrar.click(function () {

            if ((principal.options.actual + 1) >= principal.options.lista.length) {
                principal.options.lista[principal.options.actual] = undefined;
                principal.options.lista = $.grep(principal.options.lista, function (n) {
                    return (n);
                });

                principal.options.actual--;
                principal._crearVentana();
                return;
            }

            principal.options.lista[principal.options.actual] = undefined;
            principal.options.lista = $.grep(principal.options.lista, function (n) {
                return (n);
            });

            if (principal.options.actual === 0)
                principal.options.actual++;
            else
                principal.options.actual--;

            principal._crearVentana();
        });

        var textoBoton = "Siguiente";
        if (principal.options.editando || principal.options.ultimo || principal.options.unico)
            textoBoton = "Finalizar";


        var btnfinalizar = $("<button/>").addClass("btn btn-primary").text(textoBoton).appendTo(div);
        btnfinalizar.click(function (e) {
            e.preventDefault();
            $("#btnSubmit").click();

//            if (!form.validationEngine('validate')) {
//
//            } else {
//
//                if (principal.options.antesdeseguir !== undefined) {
//                    confirm(principal.options.antesdeseguir, function () {
//                        principal._grabarEnLista();
//
//                        if (principal.options.pasopaso) {
//                            if (!principal.options.ultimo)
//                                principal.element.modal("hide");
//
//                            if (principal.options.unico)
//                                principal.options.callback(principal.element, principal.options.lista[principal.options.actual]);
//                            else
//                                principal.options.callback(principal.element, principal.options.lista);
//                        } else {
//                            principal._grabarTodo();
//                        }
//                    }, principal.options.nombre);
//                } else {
//                    principal._grabarEnLista();
//
//                    if (principal.options.pasopaso) {
//                        if (!principal.options.ultimo)
//                            principal.element.modal("hide");
//
//                        if (principal.options.unico)
//                            principal.options.callback(principal.element, principal.options.lista[principal.options.actual]);
//                        else
//                            principal.options.callback(principal.element, principal.options.lista);
//                    } else {
//                        principal._grabarTodo();
//                    }
//                }
//            }
        });
    },
    _crearFormulario: function () {
        var principal = this;
        var divCuerpo = this.element.find(".modal-body");

        var form = $("<form enctype='multipart/form-data'/>");
        form.submit(function (e) {
            e.preventDefault();
            var data = new FormData(this);

            principal._grabarTodo();
        });


        form.addClass("form-horizontal");

        //por cada campo en la tabla crea un input y lo agrega al formulario
        $.each(this.options.fields, function (i, val) {
            form.append(principal._crearInputs(form, i, val));

        });
        form.append('<input type="submit" id="btnSubmit" style="display:none;">')
        form.appendTo(divCuerpo);
    },
    //---------------------------------------------------------------------

    _crearSelect: function (input, fieldoptions, principal, valorcombo) {
        valorcombo = valorcombo || "";
        input.empty();
        //input.chosen("destroy");

        if (fieldoptions.opciones.tipo !== "muchos") {
            input.append("<option value=''>Seleccione una opción</option>");
            input.find('option[value=""]').attr('selected', 'selected');
        }

        if (typeof fieldoptions.opciones.source === 'string')
        {
            $.post(fieldoptions.opciones.source, { id: valorcombo }, function (data) {
                $.each(data.Options, function (i, item) {
                    input.append("<option value='" + item.Value + "'>" + item.DisplayText + "</option>");
                });
                if (principal.options.lista[principal.options.actual] !== undefined && !input.is("input")) {
                    input.find('option[value=' + principal.options.lista[principal.options.actual][index] + ']').attr('selected', 'selected');
                    //input.trigger("change");
                }

                //carga los seleccionados de la tabla
                if (principal.options.editando) {

                    if (fieldoptions.opciones.tipo === "muchos") {

                        $.each(principal.options.info[fieldoptions.field], function (index, value) {
                            input.find('option[value=' + value + ']').attr('selected', 'selected');
                        });
                    } else {
                        input.find('option[value=' + principal.options.info[fieldoptions.field] + ']').attr('selected', 'selected');
                    }
                    input.trigger("change");
                } else
                if (principal.options.info !== undefined) {
                    input.find("option[value='" + principal.options.info[name] + "']").attr('selected', 'selected');

                }
            });
        } else {
            $.each(fieldoptions.opciones.source, function (i, item) {
                input.append("<option value='" + item.Value + "'>" + item.DisplayText + "</option>");
            });

            if (principal.options.editando) {
                input.find('option[value=' + principal.options.info[fieldoptions.field] + ']').attr('selected', 'selected');
            }

        }
    },
    //---------------Crea los inputs lo llama _crearFormulario------------- el mas complejo
    _crearInputs: function (form, index, fieldoptions) {
        var principal = this;

        if ((this.options.editando && fieldoptions.edit === true) || (!this.options.editando && fieldoptions.create === true)) {
            //creo todas la variables que voy a usar
            var tamañouno = "col-md-3";
            var tamañodos = "col-md-9";
            var label = null;
            var input = null;
            var divinput = $("<div/>").addClass(tamañodos);
            

            var leyenda = $("<p/>");
            var categoriaNombre = "";
            var name = fieldoptions.field;
            var nombrelabel = fieldoptions.title;

            label = $("<label/>").addClass(tamañouno).addClass("control-label").text(nombrelabel);
            label.attr("for", name);

            if (fieldoptions.opciones.categoria !== undefined && fieldoptions.opciones.categoria.nombre !== "") {

                categoriaNombre = fieldoptions.opciones.categoria.nombre;
            } else {

                categoriaNombre = name;

            }
            //Creo el input y
            input = $("<input/>")
                    .attr("id", name)
                    .attr("name", name)
                    .addClass("form-control")
                    .attr("type", "text");
            input.appendTo(divinput);


            if (fieldoptions.opciones.tipo === "radio")
                input.attr({"name": fieldoptions.opciones.categoria.nombre, "data-name": name});

            //Aca comienza a preguntar que tipo es-----------------------------------------------------------------------------------------------------------------------------



            if (fieldoptions.opciones.tipo === "checkbox" || fieldoptions.opciones.tipo === "radio") {

                input.attr("data-on-text", "Si")
                        .attr("data-off-text", "No")
                        //.attr("data-label-text", fieldoptions.title)
                        .attr("type", fieldoptions.opciones.tipo)
                        .attr("data-label-width", 120)
                        .attr("data-deshabilita", fieldoptions.opciones.deshabilitar)
                        .attr("data-controla", fieldoptions.opciones.controla)


                if (fieldoptions.opciones.def !== undefined && fieldoptions.opciones.def === true) {
                    input.attr("checked", true);
                }
                if (fieldoptions.opciones.deshabilitar === true) {
                    input.attr("checked", true);
                }
                if (fieldoptions.opciones.deshabilitar && principal.options.lista.length > 0) {
                    input.attr("checked", true);
                }
                if (principal.options.editando) {
                    if (principal.options.info[name] === "No")
                        input.attr("checked", false);
                    else
                    if (principal.options.info[name] === "Si")
                        input.attr("checked", true);
                    else
                        input.attr("checked", principal.options.info[name]);
                }

                input.bootstrapSwitch({
                    radioAllOff: true
                });




            }





            if (fieldoptions.opciones.tipo === 'imagen') {
                input.prop({readonly: true});

                var divPadre = $("<div/>");

                input.css("width", "70%").addClass("col-xs-5 col-md-5");

                var falsoinput = $("<input/>")
                        .attr("type", "hidden");
                falsoinput.appendTo(divinput);

                if (fieldoptions.opciones.atributos !== undefined && fieldoptions.opciones.atributos.input !== undefined) {
                    $.each(fieldoptions.opciones.atributos.input, function (i, val) {
                        falsoinput.attr(i, val);
                    });
                }

                var btnBuscar = $('<a>')
                        .addClass("btn btn-primary form-control col-xs-4 col-md-4")
                        .css("width", "30%")
                        .html('<span class="glyphicon glyphicon-zoom"></span>' + "Buscar...");


                input.appendTo(divPadre);
                btnBuscar.appendTo(divPadre);
                divPadre.appendTo(divinput);

                btnBuscar.click(function () {
                    $("#" + fieldoptions.opciones.fileId).trigger("click");
                    $("#" + fieldoptions.opciones.fileId).change(function (evt) {

                        input.val($(this).val());

                        var $this = $(this), $clone = $this.clone();
                        $this.after($clone).appendTo(form);
                    });
                });
            }





            if (fieldoptions.opciones.tipo === "autocompleteUsuarioAD") {
                input.attr("autocomp", "true");

                input.autocomplete({
                    source: function (request, response) {
                        var busqueda = this.element.val();
                        $.post(fieldoptions.opciones.source, {apellido: busqueda}, function (data) {
                            var arr = $.map(data.Options, function (el) {
                                return {
                                    label: el.DisplayText,
                                    value: el.Value
                                };
                            });
                            response(arr);
                        });
                    },
                    select: function (event, ui) {
                        event.preventDefault();
                        $(this).val(ui.item.label);
                        var idNombre = fieldoptions.opciones.nombre;
                        var apellido = fieldoptions.opciones.apellido;
                        var UserMagyp = fieldoptions.opciones.usuarioMagyp;

                        if (!ui.item) {
                            $(this).val('');
                            $('#' + idNombre).val('');
                            $('#' + apellido).val('');
                            $('#' + UserMagyp).val('');
                        } else {
                            $('#' + idNombre).val(ui.item.value.split("|")[1]);
                            $('#' + apellido).val(ui.item.value.split("|")[0]);
                            $('#' + UserMagyp).val(ui.item.value.split("|")[2]);
                        }
                    },
                    minLength: 3
                });

            }









            if (fieldoptions.opciones.tipo === "cuit") {
                input.attr("maxlength", 11);
                input.change(function () {
                    if (input.val().length === 11) {
                        input.attr("disabled", true);
                        $.blockUI({message: '<i class="fa fa-cog fa-spin fa-5x"></i><p style="font-size:14px;font-weight:bold;color:#000;margin-top:5px">CARGANDO</p>'});
                        $.post(fieldoptions.opciones.source, {cuit: input.val()}, function (data) {
                            input.attr("disabled", false);
                            fieldoptions.opciones.callback(data);
                            $.unblockUI();
                        });
                    }
                });
            }










            if (fieldoptions.opciones.tipo === "select" || fieldoptions.opciones.tipo === "muchos") {
                divinput.empty();
                if (fieldoptions.opciones.tipo === "select") {
                    input = $("<select/>");
                } else {
                    input = $("<select multiple/>");
                    input.attr("data-placeholder", "Seleccione una opción");
                }
                input.appendTo(divinput);
                input.attr("id", name);
                input.attr("name", name);
                input.css("margin-left", "0px");
                input.addClass("chosen-select");
                input.addClass("form-control");


                this._crearSelect(input, fieldoptions, principal);
                if (fieldoptions.opciones.dependeDe !== null) {
                    form.find("select[name='" + fieldoptions.opciones.dependeDe + "']").change(function () {
                        principal._crearSelect(input, fieldoptions, principal, $(this).val());
                    });
                }
            }

            if (fieldoptions.opciones.tipo === "multiple") {
                divinput.empty();
                input = $("<section/>");
                input.appendTo(divinput);
                $.ajax({
                    url: fieldoptions.opciones.source,
                    type: 'POST',
                    async: false,
                    success: function (data) {
                        $.each(data.Options, function (i, item) {

                            var multi = $("<input value='" + item.Value + "' name='" + name + "'/>");
                            multi.attr("data-multiple", true);
                            multi.attr("data-on-text", "Si");
                            multi.attr("data-off-text", "No");
                            multi.attr("data-label-text", item.DisplayText);
                            multi.attr("type", "checkbox");

                            if (principal.options.info !== undefined && principal.options.info[name] !== undefined) {
                                if (principal.options.info[name].indexOf(item.Value) !== -1) {
                                    multi.attr("checked", true);
                                }
                            }

                            if (principal.options.lista[principal.options.actual] !== undefined) {
                                if (principal.options.lista[principal.options.actual][index].indexOf("" + item.Value) !== -1)
                                    multi.attr("checked", true);
                            }

                            input.append(multi);
                        });
                    }
                });
            }









            if (fieldoptions.opciones.tipo === "multilinea") {
                divinput.empty();
                input = $("<textarea/>")
                        .attr("id", name)
                        .attr("name", name)
                        .addClass("form-control");
                input.appendTo(divinput);
            }








            if (fieldoptions.opciones.tipo === "mapa") {
                input.attr("data-mapa", true);

                setTimeout(function () {
                    var mapCanvas = form.find("input[data-mapa=true]").closest("div");
                    mapCanvas.css("height", "480px");

                    var map = new google.maps.Map(mapCanvas[0], {
                        zoom: 4,
                        center: new google.maps.LatLng(-38.416097, -63.616671999999994),
                        mapTypeId: google.maps.MapTypeId.ROADMAP
                    });

                    var marker = false;
                    google.maps.event.addListener(map, 'click', function (event, latLng) {

                        if (marker === false) {
                            marker = new google.maps.Marker({
                                position: event.latLng,
                                map: map
                            });
                        } else {
                            marker.setPosition(event.latLng);
                        }

                        var myLatLng = event.latLng;
                        var lat = myLatLng.lat();
                        var lng = myLatLng.lng();

                        $.post(fieldoptions.opciones.url, {lati: lat, longi: lng})
                                .done(function (result) {
                                    form.find("input[name='Latitud']").val(lat);
                                    form.find("input[name='Longitud']").val(lng);
                                    form.find("input[name='Partido']").val(result.Departamento);
                                    form.find("input[name='PartidoId']").val(result.DepartamentoId);
                                    form.find("input[name='Provincia']").val(result.Provincia);
                                    form.find("input[name='ProvinciaId']").val(result.ProvinciaId);
                                });
                    });

                    if (principal.options.lista[principal.options.actual] !== undefined) {
                        var latitud = principal.options.lista[principal.options.actual].Latitud;
                        var longitud = principal.options.lista[principal.options.actual].Longitud;

                        var myLatLong = new google.maps.LatLng(latitud, longitud);
                        marker = new google.maps.Marker({
                            position: myLatLong,
                            map: map
                        });
                    }

                    if (principal.options.editando) {
                        var latitud = principal.options.info.Latitud;
                        var longitud = principal.options.info.Longitud;

                        var myLatLong = new google.maps.LatLng(latitud, longitud);
                        marker = new google.maps.Marker({
                            position: myLatLong,
                            map: map
                        });

                    }
                }, 500);

                if (fieldoptions.opciones.leyenda !== undefined) {
                    var leyenda = $("<p/>").text(fieldoptions.opciones.leyenda);
                    leyenda.appendTo(divinput);
                }

                var interno = $("<div/>");
                interno.append(input);
                input = interno;
            }







            if (fieldoptions.opciones.tipo === "inputcolor") {
                input.colorpicker();
            }






            if (fieldoptions.opciones.tipo === "date") {
                $.fn.datepicker.dates = "es";
                input.datepicker({
                    autoclose: true,
                    format: "dd/mm/yyyy",
                    language: "es",
                });
            }						if (fieldoptions.opciones.tipo === "datetime") {                input.addClass("form_datetime");				input.datetimepicker(fieldoptions.opciones.datetime);            }
						







            if (fieldoptions.opciones.tipo === "monto") {
                var divPadre = $("<div/>");

                input.css("width", "70%")
                        .addClass("col-xs-5 col-md-5");

                var selectMonedas = $("<select/>")
                        .attr("name", "combo" + fieldoptions.field)
                        .css("width", "30%")
                        .addClass("form-control col-xs-4 col-md-4");

                $("<option value=''>Seleccione una opción</option>").appendTo(selectMonedas);
                $.post(fieldoptions.opciones.monedas, function (data) {
                    $.each(data.Options, function (i, item) {
                        selectMonedas.append("<option value='" + item.Value + "'>" + item.DisplayText + "</option>");
                    });

                    if (principal.options.editando) {
                        selectMonedas.val(principal.options.info["combo" + fieldoptions.field]);
                    }
                });


                if (fieldoptions.opciones.diseñoCombo !== undefined && fieldoptions.opciones.diseñoCombo.clasesInput !== undefined) {
                    selectMonedas.addClass(fieldoptions.opciones.diseñoCombo.clasesInput);
                }

                selectMonedas.appendTo(divPadre);
                input.appendTo(divPadre);
                divPadre.appendTo(divinput);
            }






            //Mete el placeholder
            if (fieldoptions.opciones.placeholder !== undefined) {
                input.attr("placeholder", fieldoptions.opciones.placeholder);
            }
            //Adhiere las clases que se hayan puesto en opciones.diseño.clasesInput y los atributos que se hayan puesto en opciones.atributos.input
            if (fieldoptions.opciones.diseño !== undefined) {
                if (fieldoptions.opciones.diseño.clasesInput !== undefined) {
                    input.addClass(fieldoptions.opciones.diseño.clasesInput);
                }
                if (fieldoptions.opciones.diseño.attr !== undefined) {
                    $.each(fieldoptions.opciones.diseño.attr, function (i, val) {
                        input.attr(i, val);
                    });
                }
                if (fieldoptions.opciones.diseño.css !== undefined) {
                    $.each(fieldoptions.opciones.diseño.css, function (i, val) {
                        input.css(i, val);
                    });
                }
                if (fieldoptions.opciones.diseño.cssLabel !== undefined) {
                    $.each(fieldoptions.opciones.diseño.cssLabel, function (i, val) {
                        label.css(i, val);
                    });
                }
                if (fieldoptions.opciones.diseño.clasesLabel !== undefined) {
                    label.addClass(fieldoptions.opciones.diseño.clasesLabel);
                }
                if (fieldoptions.opciones.diseño.clasesDiv !== undefined) {

                    divinput.addClass(fieldoptions.opciones.diseño.clasesDiv);
                }
            }

            if (fieldoptions.opciones.atributos !== undefined && fieldoptions.opciones.atributos.input !== undefined) {
                $.each(fieldoptions.opciones.atributos.input, function (i, val) {
                    input.attr(i, val);
                });
            }


            if (principal.options.editando && fieldoptions.opciones.editar !== undefined && fieldoptions.opciones.editar.atributos) {
                $.each(fieldoptions.opciones.editar.atributos, function (i, val) {
                    input.attr(i, val);
                });
            }




            //console.log(fieldoptions.opciones.editar.atributos);






            //ACA BUSCO EL GRUPO CON LA CATEGORIA. SINO EXISTE LO CREO
            if (form.find("div[data-categoria='" + categoriaNombre + "']").length > 0) {
                var grupo = form.find("div[data-categoria='" + categoriaNombre + "']");
            } else {
                var grupo = $("<div/>").addClass("form-group");
                grupo.attr("data-categoria", categoriaNombre);
                //asigna el diseño que le pongamos al div
                if (fieldoptions.opciones.diseñoCombo !== undefined) {
                    if (fieldoptions.opciones.diseñoCombo.css !== undefined) {
                        $.each(fieldoptions.opciones.diseñoCombo.css, function (i, val) {
                            grupo.css(i, val);
                        })
                    }
                    if (fieldoptions.opciones.diseñoCombo.clases !== undefined) {
                        grupo.addClass(fieldoptions.opciones.diseñoCombo.clases);
                    }
                }
                
                if (fieldoptions.opciones.diseño !== undefined && fieldoptions.opciones.diseño.cssDiv !== undefined) {
                $.each(fieldoptions.opciones.diseño.cssDiv, function (i, val) {
                    grupo.css(i, val);
                });
            }
            }






            //Si estamos editando cargo los datos que estan en la tabla
            if (this.options.editando) {
                //los selec y muchos le carga los datos en crearSelect porque tiene que esperar que le lleguen los datos del servidor
                if (fieldoptions.opciones.tipo !== "muchos" && fieldoptions.opciones.tipo !== "select") {

                    input.val(principal.options.info[name]);
                }

                if (fieldoptions.opciones.tipo === "multilinea") {
                    var testo = principal.options.info[name];
                    input.val(testo.replace(new RegExp("<br/>", 'g'), "\n"));
                }
            }




            //si no esta editando y nos dieron un valor predeterminado
            if (fieldoptions.opciones.valor !== undefined && !this.options.editando)
                input.val(fieldoptions.opciones.valor);


            //Si tiene una leyenda se la agrega
            if (fieldoptions.opciones.leyenda !== undefined) {
                leyenda.text(fieldoptions.opciones.leyenda);
                leyenda.appendTo(divinput);
            }
            //le meto el evento change a la funcion
            //if (fieldoptions.opciones.change != undefined) {
            //    $(document).on("change", "#" + input.attr("id"), fieldoptions.opciones.change);
            //    //input.change(fieldoptions.opciones.change);
            //}
            //Se agrega el evento para ocultar
            if (fieldoptions.opciones.ocultoCon !== undefined) {
                var inputaocultar = form.find("#" + fieldoptions.opciones.ocultoCon.nombre);

                if (inputaocultar.attr("type") === "checkbox" || inputaocultar.attr("type") === "radio") {
                    inputaocultar.on('switchChange.bootstrapSwitch', function () {
                        principal._ocultarConFunc(form, fieldoptions, grupo, true);
                    });
                } else {
                    inputaocultar.change(function () {
                        principal._ocultarConFunc(form, fieldoptions, grupo, false);
                    });
                }

                if (eval(fieldoptions.opciones.ocultoCon.condicion)) {
                    grupo.hide();
                }

                if (principal.options.editando) {
                    principal._ocultarConFunc(form, fieldoptions, grupo, inputaocultar.attr("type") === "checkbox" || inputaocultar.attr("type") === "radio");
                }

            }

            if (input.attr("type") === "checkbox" || input.attr("type") === "radio") {
                //descomentar si se usa un change en bootstrapswitch tipo
                input.on('switchChange.bootstrapSwitch', fieldoptions.opciones.change);
            } else {
                if (fieldoptions.opciones.change !== undefined) {
                    input.change(fieldoptions.opciones.change);
                }
            }




            //Finalmente Agrego el divinput y el label al grupo y retorno el grupo

            label.appendTo(grupo);
            divinput.appendTo(grupo);
        }
        return grupo;
    },
    //---------------------------------------------------------------------


    _ocultarConFunc: function (form, fieldoptions, grupo, isswitch) {
        var inputevaluar = form.find("#" + fieldoptions.opciones.ocultoCon.nombre);
        var condicion = fieldoptions.opciones.ocultoCon.condicion;
        if (isswitch) {
            if (inputevaluar.bootstrapSwitch('state') === condicion) {
                grupo.show('slow');
            } else
                grupo.hide('slow');
        } else {
            if (!eval(fieldoptions.opciones.ocultoCon.condicion))
                grupo.show('slow');
            else
                grupo.hide('slow');
        }
    },
    _crearObjeto: function (form) {
        var fields = {};
        form.find(":input").each(function () {

            var valor = $(this).val();
            if ($(this).attr("type") === "checkbox") {
                valor = $(this).bootstrapSwitch('state');
            }

            if ($(this).attr("type") === "radio") {
                var name = $(this).data("name");
                valor = $(this).bootstrapSwitch('state');
                fields[name] = valor;
                //valor = $(this).bootstrapSwitch('state');
            }

            if ($(this).attr("autocompc") === "true" && $(this).attr("id") != "codep") {
                if ($(this).attr("name") !== undefined)
                    valor = form.find('#id' + $(this).attr("name")).val();
            }

            if ($(this).is('select')) {
                valor = $(this).val();
            }

            if ($(this).data("multiple")) {
                if ($(this).is(":checked")) {
                    if (fields[this.name] === undefined) {
                        fields[this.name] = [$(this).val()];
                    } else {
                        var ar = fields[this.name];
                        ar.push($(this).val());
                    }
                }
            } else {
                fields[this.name] = valor;
            }

        });

        return fields;
    },
    _grabarEnLista: function () {
        var form = this.element.find("form");

        var objeto = this._crearObjeto(form);
        this.options.lista[this.options.actual] = objeto;
        this.options.lista = $.grep(this.options.lista, function (n) {
            return (n);
        });
    },
    _grabarTodo: function () {
        var principal = this;
//        $.each(principal.element.find("button"), function (i, val) {
//            $(this).attr("disabled", true);
//        });
        //var todo = JSON.stringify(principal.options.editando ? principal.options.lista[principal.options.actual] : principal.options.lista);

        var form = this.element.find("form");
        var todo = new FormData(form[0]);

        if (principal.options.url !== null) {
            $.ajax({
                url: (principal.options.editando ? principal.options.urlEditar : principal.options.url) + (principal.options.parametrosGet === null ? "" : principal.options.parametrosGet),
                data: todo,
                type: 'POST',
                processData: false,
                contentType: false,
                cache: false,
                dataType: "json",
                success: function (data) {
                    if (data.error === false) {
                        alert("Se han grabado satisfactoriamente todos los datos!", principal.options.nombre, function () {
                            principal.element.modal("hide");
                            principal.options.lista = [];
                            principal.options.actual = 0;

                            principal.options.grabado(principal.options.editando, data, principal.options.editandoIndex, principal.options.indexPadre);
                        });
                    } else {
                        alert(data.mensaje, principal.options.nombre);

                        $.each(principal.element.find("button"), function (i, val) {
                            $(this).attr("disabled", false);
                        });
                    }
                }
            });
        }
    },
    _isHTML: function (str) {
        var a = document.createElement('div');
        a.innerHTML = str;
        for (var c = a.childNodes, i = c.length; i--; ) {
            if (c[i].nodeType === 1)
                return true;
        }
        return false;
    },
    _deparam: function (querystring) {
        // remove any preceding url and split
        querystring = querystring.substring(querystring.indexOf('?') + 1).split('&');
        var params = {}, pair, d = decodeURIComponent, i;
        // march and parse
        for (i = querystring.length; i > 0; ) {
            pair = querystring[--i].split('=');
            params[d(pair[0])] = d(pair[1]);
        }

        return params;
    },
    _create: function () {

        this.options.actual = 0;
        this.options.lista = [];

    },
    Set: function (donde, que) {
        this.options[donde] = que;
    },
    SetParametros: function (parametros) {
        this.options.parametrosGet = parametros;
    },
    SetIndexPadre: function (indexPadre) {
        this.options.indexPadre = indexPadre;
    },
    GetVariable: function (variable) {
        return this.options[variable];
    }
});