/** JTABLE Multiple toolbar search extension 

**/
(function ($) {
    var base = {
        _createToolBar: $.hik.jtable.prototype._createToolBar,
        _addRowToTableHead: $.hik.jtable.prototype._addRowToTableHead,
        _addColumnsToHeaderRow: $.hik.jtable.prototype._addColumnsToHeaderRow,
        _addCellsToRowUsingRecord: $.hik.jtable.prototype._addCellsToRowUsingRecord,
        _createAddRecordDialogDiv: $.hik.jtable.prototype._createAddRecordDialogDiv,
        _reloadTable: $.hik.jtable.prototype._reloadTable,
    }
    $.extend(true, $.hik.jtable.prototype, {
        options: {
            buscar: false,
            buscarReset: true,
            buscarResetTexto: 'Limpiar',
            buscarTexto: 'Buscar',
            buscarIcon: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAQpJREFUeNpi/P//PwMlgAXG0FFTcQBSBUDsDxX6CMQbQGJXbt35gMsAJqjmBCC1H4gFgDgQiB2BuAGIQYY+AMob4DKAUVtVGSR5HogbgTY1IEsCNYIMPABiA+UMcLkA5OyL6JqhmkBODwBifaBhAbgMUID6FSsAGvIAZAEQ43QBMQBvIH6AugIrgIaDPSgwcRkAcn48npBeAcTfcHmTCejHBUB6Iyi0gYYUINmsAMQgTe5AzAWNVsxohKVEoOIJQCofTf4hNJZA3pgPxAvRExYjclKG+tcAKQYOoKXUDdCwcIAZwkhKXoCGEywsAoCGXGAkNTMhpU5QzAUwkpMboYaAwkyBkdLsDBBgAOrrYWfgk63wAAAAAElFTkSuQmCC',

            exportarTexto: 'Exportar Ultima busqueda',
            exportarIcon: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAedJREFUeNqUUz1IW1EYPe+Rl6oU0RpTKEYRBweROrgEUZwCNk0hNGCnbBqjUBeXBqqgSDGbDgkSyBDM1qFaobRQt3awrUsnEUQSCMlLEFGjvpeX3r7vyg0R0qQeOHxwOff7Ofe7EmMMhEQiETFDEPUR9fv9s3dOKAExHo+zcrlcl6QxGRF3iLJIZBgGj/l8HqqqcuZyOWSzWU6Cx+MhXTAWi0XEvUqCUqnEoyRJdyjLMo8EXdfh9XpJG4yY4Pq5bTfDf2Dj+Q7S6TQURYHVakUymeSeIPB+gjUCaWp5Eg6HmcXQDCyuv8Nx8QiZ6xPsrezxiu7VFzi9OMVj1g7bUwOFQgHixSja7XY+kqzfGFiefwPn8BAetikYWxjBs5ALhfMMrM0MH9Y+gjTVflAkaJoGi3Z1a96c6zWkpj/Y//YDJ6kUOjs7sLv0GTouQRphZDWoA4t+bVQOdr9/wfCgE3qLgvTZMaa3AnAoT0AaqipGEKAOZNHBeGgMalHF15+f0OfoQm+3Awf7v/B2cqnSQa1n5SNMLQTQ/uARWlqbwC5kHP4+hKOnC922Il4tvjQr1R6Be0AGxdY36+7A6PxAzQS0fPwZnTP9DRfpXyZWPoXP5+NL0+hDCRLojqXqM0XNPQ/ifoj+FWAAycd1nq8xbuYAAAAASUVORK5CYII=',
            exportarUrl: '',
            descargarUrl: '',
            errorDescarga: 'Error al descargar el archivo',
            errorBusqueda: 'Error al realizar la busqueda',
            ListadoTexto: 'Volviendo a generar la consulta',
            ExportarTexto: 'Exportando Lista',
            autocomplete: false
        },
        _reloadTable: function (completeCallback) {
            var self = this;

            var completeReload = function (data) {
                self._hideBusy();

                //Show the error message if server returns error
                if (data.Result != 'OK') {
                    self._showError(data.Message);
                    return;
                }

                //Re-generate table rows
                self._removeAllRows('reloading');
                self._addRecordsToTable(data.Records);

                self._onRecordsLoaded(data);

                //Call complete callback
                if (completeCallback) {
                    completeCallback(data);
                }
            };

            self._showBusy(self.options.messages.loadingMessage, self.options.loadingAnimationDelay); //Disable table since it's busy
            self._onLoadingRecords();

            //listAction may be a function, check if it is
            if ($.isFunction(self.options.actions.listAction)) {

                //Execute the function
                var funcResult = self.options.actions.listAction(self._lastPostData, self._createJtParamsForLoading());

                //Check if result is a jQuery Deferred object
                if (self._isDeferredObject(funcResult)) {
                    funcResult.done(function (data) {
                        completeReload(data);
                    }).fail(function () {
                        self._showError(self.options.messages.serverCommunicationError);
                    }).always(function () {
                        self._hideBusy();
                    });
                } else { //assume it's the data we're loading
                    completeReload(funcResult);
                }

            } else { //assume listAction as URL string.

                //Generate URL (with query string parameters) to load records
                var loadUrl = self._createRecordLoadUrl();

                //Load data from server using AJAX
                self._ajax({
                    url: loadUrl,
                    data: self._lastPostData,
                    success: function (data) {
                        completeReload(data);
                    },
                    error: function () {
                        self._hideBusy();
                        self._showError(self.options.messages.serverCommunicationError);
                    }
                });

            }
        },
        _createToolBar: function () {
            var self = this;
            base._createToolBar.apply(self);

            if (this.options.buscar === true) {
                this._addToolBarItem({
                    icon: this.options.buscarIcon,
                    text: this.options.buscarTexto,
                    click: function () {
                        $(".jtable-main-container table thead tr:eq(1)").toggle();
                    }
                });
            }

            if (this.options.exportarUrl && this.options.descargarUrl) {

                this._addToolBarItem({
                    icon: this.options.exportarIcon,
                    text: this.options.exportarTexto,
                    click: function () {

                        self._showBusy(self._formatString(self.options.ListadoTexto, 1, 2));
                        var loadUrl = self.options.actions.listAction;

                        //Load data from server using AJAX
                        self._ajax({
                            url: loadUrl,
                            data: self._lastPostData,
                            success: function (data) {
                                self._showBusy(self._formatString(self.options.ExportarTexto, 1, 2));
                                if (data.Result == "OK") {
                                    var cols = self._fieldList;
                                    var datos = data.Records;


                                    var cols = [];
                                    $.each(self._fieldList, function (i, val) {
                                        if (self.options.fields[val].excluir != true) {
                                            if (self.options.fields[val].title)
                                                cols.push(self.options.fields[val].title);
                                            else
                                                cols.push(val);
                                        }
                                    });
                                    var todo = JSON.stringify({ cols: cols, datos: datos });

                                    self._ajax({
                                        url: self.options.exportarUrl,
                                        data: todo,
                                        dataType: 'json',
                                        contentType: 'application/json',
                                        success: function (d) {
                                            self._hideBusy();

                                            if (d.success)
                                                window.location = self.options.descargarUrl + "?fName=" + d.fName;
                                        },
                                        error: function () {
                                            self._hideBusy();
                                            self._showError(self.options.errorDescarga);
                                        }
                                    });
                                }
                                else {
                                    self._hideBusy();
                                    self._showError(self.options.errorBusqueda);
                                }

                                //completeReload(data);
                            },
                            error: function () {
                                self._hideBusy();
                                self._showError(self.options.messages.serverCommunicationError);
                            }
                        });
                    }
                });
            }
        },
        _addRowToTableHead: function (thead) {
            var self = this;

            var tr = $('<tr></tr>');
            tr.appendTo(thead);

            this._addColumnsToHeaderRow(tr);

            if (this.options.buscar) {

                tr = $('<tr></tr>')
                    .css('display', 'none');
                tr.appendTo(thead);

                for (val in self.options.fields) {
                    var nombre = val;
                    var field = self.options.fields[nombre];
                    if (field.list != false) {
                        if (field.excluirBusqueda == true)
                            $('<th></th>').appendTo(tr);
                        else {
                            var th = self._addColumnToHeaderBusqueda(val, self.options.fields[val]);
                            th.appendTo(tr);
                        }
                    }
                }

                if (this.options.buscarReset) {

                    var th = $('<th></th>')
                        .attr('colspan', $(".jtable-command-column-header").length);

                    var button = $('<button class="btn">' + this.options.buscarResetTexto + '</button>')
                        .css('display', 'block')
                        .css('margin', 'auto');

                    button.appendTo(th);

                    button.click(function () {
                        $.each($('.jtable-busqueda'), function (i, val) {
                            if ($(val).is('select')) {
                                $(val).val('-1');
                            }
                            if ($(val).is('input')) {
                                $(val).val('');
                            }
                        });
                        self.load({});
                    });

                    tr.append(th);

                }
            }
        },
        _addColumnToHeaderBusqueda: function (nombre, field) {
            var self = this;
            this._createErrorDialogDiv();

            var th = $('<th></th>');
            if (field.dropdown) {

                if (!this.options.sufijo && this.options.prefijo && this.dropdown == true) {
                    self._showError("No se encuentra el ID del campo " + nombre);
                    return th;
                }

                //if (this.options.fields[nombre].idTabla)
                //idName = this.options.fields[nombre].idTabla;
                var idName = null;
                if (this.options.sufijo)
                    idName = nombre + self.options.sufijo;
                else if (this.options.prefijo)
                    idName = this.options.prefijo + (nombre.charAt(0).toUpperCase() + nombre.slice(1));


                if (this.options.fields[nombre].autocomplete) {

                    var idTabla = idName;
                    if (this.options.fields[nombre].idTabla)
                        idTabla = this.options.fields[nombre].idTabla;

                    var input = $('<input id="' + idTabla + '"/>')
                        .attr('type', 'text')
                        .css('width', '100%')
                        .addClass('jtable-busqueda');
                    console.log(this.options.fields);
                    var url = this.options.fields[idName].options;

                    //Revisar si es funcion
                    if ($.isFunction(url)) {
                        var data = {};
                        var valores = {};
                        var dependedValues = {};
                        dependedValues[this.options.fields[idName].dependsOn] = null;
                        valores["dependedValues"] = dependedValues;
                        url = url(valores);
                    }



                    input.autocomplete(
                    {
                        source: function (request, response) {
                            var busqueda = this.element.val();
                            $.post(url, { busqueda: busqueda }, function (data) {

                                var arr = $.map(data.Options, function (el) {
                                    console.log(el);
                                    return {
                                        label: el.DisplayText,
                                        value: el.Value
                                    };
                                });
                                response(arr);
                            });
                        },
                        minLength: 1,
                        select: function (event, ui) {
                            event.preventDefault();
                            console.log(ui);
                            $(this).val(ui.item.label);
                            $(this).data('id', ui.item.value);
                            $(this).data('EsValido', true);
                        },
                        change: function (e, ui) {
                            if (!ui.item)
                                $(this).val('');
                        }
                    });

                    input.appendTo(th);
                    this._createEvents(input, true);
                }
                else {

                    var idTabla = idName;
                    if (this.options.fields[nombre].idTabla)
                        idTabla = this.options.fields[nombre].idTabla;

                    var select = $('<select id="' + idTabla + '"/>')
                        .css('width', '95%')
                        .addClass('jtable-busqueda');

                    select.appendTo(th);
                    this._createEvents(select);

                    var url = this.options.fields[idName].options;

                    $.ajax({
                        type: "POST",
                        url: url,
                        async: false,
                        success: function (data) {
                            if (data.Result != 'OK') {
                                self._showError(data.Message);
                                return;
                            }

                            var tieneVacio = false;
                            options = data.Options;
                            select.empty();
                            $.each(options, function (i, val) {
                                $('<option value=' + val.Value + '>' + val.DisplayText + '</option>')
                                    .appendTo(select);
                            });
                        },
                        error: function (data) {
                            var errMessage = self._formatString(self.options.messages.cannotLoadOptionsFor, nombre);
                            self._showError(errMessage);
                        }
                    });
                }
            }
            else {

                var input = $('<input id="' + nombre + '"/>')
                    .attr('type', 'text')
                    .css('width', '100%')
                    .addClass('jtable-busqueda');

                if (field.type == "date") {
                    var displayFormat = field.displayFormat || this.options.defaultDateFormat;
                    input.datepicker({
                        dateFormat: displayFormat,
                        yearRange: "-100:+1",
                        showButtonPanel: true
                    });
                }

                input.appendTo(th);

                if (this.options.fields[nombre].ejecutarDespues) {
                    input.on('change', function () {

                        var display2 = {};
                        display2[nombre] = input.val();

                        var todo = $.param(display2);
                        self.load(todo, function (data) {
                            self.options.fields[nombre].ejecutarDespues(data, input);
                        });

                    });


                } else {
                    this._createEvents(input);
                }

            }

            if (field.visibility == 'hidden') {
                th.hide();
            }

            return th;
        },
        _createEvents: function (input, esAutoComplete) {
            var self = this;

            input.on('change', function (e) {
                self._CargarEvento(e, esAutoComplete);
            });

        },
        _CargarEvento: function (e, esAutoComplete) {
            var self = this;

            var q = [];
            var opt = [];

            $.each($('.jtable-busqueda'), function (i, busqueda) {
                busqueda = $(busqueda);
                var val = busqueda.val();
                if (val != -1 && val != '') {
                    var id = busqueda.attr('id');
                    opt.push(id);

                    if (esAutoComplete) {

                        console.log(busqueda);
                        if (busqueda.data('EsValido') != undefined || busqueda.data('EsValido') != null)
                            q.push(busqueda.data('id'))
                        else
                            q.push(-1);
                    }
                    else
                        q.push(val);

                }
            });



            var display2 = {};
            for (var x = 0; x < opt.length; x++) {
                display2[opt[x]] = q[x];
            }


            //var todo = JSON.stringify(display2);
            var todo = $.param(display2);
            self.load(todo);
        },
        _addColumnsToHeaderRow: function (tr) {
            var self = this;

            base._addColumnsToHeaderRow.apply(this, arguments);
            if (this.options.actions.otrasAction != undefined) {

                $.each(this.options.actions.otrasAction, function (i, val) {
                    tr.append(self._createEmptyCommandHeader());
                });
            }
        },
        _addCellsToRowUsingRecord: function (row) {
            var self = this;
            base._addCellsToRowUsingRecord.apply(this, arguments);

            if (self.options.actions.otrasAction != undefined) {


                $.each(this.options.actions.otrasAction, function (i, val) {

                    var id = row.data('record-key');

                    if (val.columna) {
                        var record = self;
                        if (self._getDisplayTextForRecordField(row.data('record'), val.columna))
                            self._otraAccionAgregarBoton(row, val);
                    }
                    else {
                        self._otraAccionAgregarBoton(row, val);
                    }
                });
            }
        },
        _otraAccionAgregarBoton: function (row, accion) {
            var self = this;
            var span = $('<span></span>').html(accion.texto);
            var button = $('<button title="' + accion.texto + '"></button>')
                .addClass('jtable-command-button jtable-otra-command-button')
                .css('background', 'url(' + accion.icono + ') no-repeat scroll 0% 0% transparent')
                .css('width', '16px')
                .css('height', '16px')
                .append(span);

            $('<td></td>')
                .addClass('jtable-command-column')
                .append(button)
                .appendTo(row);

            if (accion.click) {
                button.click(function () {

                    accion.click(row.data('record'));
                });
            }
            else if (accion.accion) {
                button.click(function (e) {
                    self._otraAccionButtonClickedForRow(row, accion.accion);
                });
            }
        },
        _otraAccionButtonClickedForRow: function (row, accion) {
            var self = this;
            var accionDlg = $('<div><p><span class="ui-icon ui-icon-alert" style="float:left; margin:0 7px 20px 0;"></span><span class="jtable-delete-confirm-message">' + accion.leyenda + '</span></p></div>').appendTo(self._$mainContainer);


            var id = row.data('record-key');
            //Prepare dialog
            accionDlg.dialog({
                autoOpen: false,
                show: self.options.dialogShowEffect,
                hide: self.options.dialogHideEffect,
                modal: true,
                title: accion.titulo,
                buttons:
                        [
                        {
                            text: self.options.messages.cancel,
                            click: function () {
                                accionDlg.dialog("close");
                            }
                        },
                        {
                            text: accion.botonAccion,
                            click: function () {
                                self._showBusy(self._formatString(accion.ocupadoTexto));
                                var accionUrl = accion.url;
                                var loadUrl = self.options.actions.listAction;

                                var colName = self._keyField;
                                $.post(accionUrl, { id: id }, function (data) {
                                    self._hideBusy();
                                    accionDlg.dialog('close');

                                    if (data.Result == "OK") {
                                        if (accion.actualizarTabla) {
                                            //self._showUpdateAnimationForRow(data.Record);
                                            self._reloadTable();
                                        }

                                        self._showUpdateAnimationForRow(row);
                                    }
                                    else {
                                        self._showError(data.Message);
                                    }

                                })
                                .fail(function () {
                                    self._hideBusy();
                                    self._showError(self.options.messages.serverCommunicationError);
                                });

                            }
                        }]
            });
            accionDlg.dialog('open');

        },
        _createAddRecordDialogDiv: function () {

            if (this.options.actions.createActionMostrar != false) {
                base._createAddRecordDialogDiv.apply(this);
            }
        }
    });

})(jQuery);
(function ($) {
    Date.prototype.addDays = function (days) {
        var dat = new Date(this.valueOf());
        dat.setDate(dat.getDate() + days);
        return dat;
    }
    //.chunk(5).join('$');
    String.prototype.chunk = function (n) {
        var ret = [];
        for (var i = 0, len = this.length; i < len; i += n) {
            ret.push(this.substr(i, n))
        }
        return ret
    };
    Date.prototype.formatPiola = function () {
        return this.getDate() +
            "/" + (this.getMonth() + 1) +
            "/" + this.getFullYear();
    }
})(jQuery);

(function ($) {
    window.confirm = function (mensaje, callback, titulo) {
        titulo = titulo || 'Confirmación'

        var ConfirmDiv = $("<div />");

        if (isHTML(mensaje))
            ConfirmDiv.html(mensaje);
        else
            ConfirmDiv.text(mensaje);

        ConfirmDiv.dialog({
            autoOpen: false,
            //width: 'auto',
            minWidth: '300',
            draggable: false,
            modal: true,
            resizable: false,
            title: titulo,
            buttons:
            [
                {
                    text: "Aceptar",
                    click: function () {
                        ConfirmDiv.dialog('close');
                        callback();
                        return true;
                    }
                },
                {
                    text: "Cancelar",
                    click: function () {
                        ConfirmDiv.dialog('close');
                        return false;
                    }
                }
            ]
        });

        ConfirmDiv.dialog('open');
    };

    window.alert = function (mensaje, titulo) {
        titulo = titulo || 'Mensaje del sistema'

        var ConfirmDiv = $("<div />");

        if (isHTML(mensaje))
            ConfirmDiv.html(mensaje);
        else
            ConfirmDiv.text(mensaje);

        ConfirmDiv.dialog({
            autoOpen: false,
            //width: 'auto',
            minWidth: '300',
            draggable: false,
            modal: true,
            resizable: false,
            title: titulo,
            buttons:
            [
                {
                    text: "Aceptar",
                    click: function () {
                        ConfirmDiv.dialog('close');
                        return true;
                    }
                }
            ]
        });

        ConfirmDiv.dialog('open');
    };

    function isHTML(str) {
        var a = document.createElement('div');
        a.innerHTML = str;
        for (var c = a.childNodes, i = c.length; i--;) {
            if (c[i].nodeType == 1) return true;
        }
        return false;
    }
})(jQuery);