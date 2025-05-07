//for Xls
function fnExcelParmReport(TableToExport, profile, parameter) {
    var tab_text = '';
    var $table = $(TableToExport);
    var $thead = $table.find('thead');
    var $tbody = $table.find('tbody');
    var $tfoot = $table.find('tfoot');
    var columncount = $thead.find('th').length;
    tab_text += '<table border="0.5px">';
    tab_text += "<tr><td align='left' valign='top' colspan='" + columncount + "'>" + profile + "<br/>" + parameter + "</td></tr>";
    debugger;
    if ($thead.length > 0) {
        $thead.find('tr').each(function () {
            var $row = $(this);
            tab_text += "<tr>";
            debugger;
            $row.find('th').each(function () {
                var $cell = $(this);
                if ($cell.css('display') !== 'none' && !$cell.attr('hidden')) {
                    var colspan = $cell.attr('colspan') || 1;
                    var rowspan = $cell.attr('rowspan') || 1;
                    var cellContent = $cell.html().replace(/<p[^>]*>/gi, "").replace(/<\/p>/gi, "<br>");
                    tab_text += "<th colspan='" + colspan + "' rowspan='" + rowspan + "'>" + cellContent + "</th>";
                    debugger;
                }
            });
            tab_text += "</tr>";
        });
    }

    if ($tbody.length > 0) {
        $tbody.find('tr').each(function () {
            var $row = $(this);
            tab_text += "<tr>";
            $row.find('th, td').each(function () {
                var $cell = $(this);
                if ($cell.css('display') !== 'none' && !$cell.attr('hidden')) {
                    var colspan = $cell.attr('colspan') || 1;
                    var rowspan = $cell.attr('rowspan') || 1;
                    var cellContent = $cell.is('td') && $cell.find('input').length > 0
                        ? $cell.find('input').val()
                        : $cell.html().replace(/<p[^>]*>/gi, "").replace(/<\/p>/gi, "<br>");
                    if ($cell.is('th')) {
                        tab_text += "<th colspan='" + colspan + "' rowspan='" + rowspan + "' valign='top'>" + cellContent + "</th>";
                    } else {
                        tab_text += "<td colspan='" + colspan + "' rowspan='" + rowspan + "' valign='top'>" + cellContent + "</td>";
                    }
                }
            });
            tab_text += "</tr>";
        });
    }

    if ($tfoot.length > 0) {
        $tfoot.find('tr').each(function () {
            var $footerRow = $(this);
            tab_text += "<tr>";
            $footerRow.find('td').each(function () {
                var $footerCell = $(this);
                if ($footerCell.css('display') !== 'none' && !$footerCell.attr('hidden')) {
                    var colspan = $footerCell.attr('colspan') || 1;
                    var rowspan = $footerCell.attr('rowspan') || 1;
                    var cellContent = $footerCell.is('td') && $footerCell.find('input').length > 0
                        ? $footerCell.find('input').val()
                        : $footerCell.html().replace(/<p[^>]*>/gi, "").replace(/<\/p>/gi, "<br>");
                    tab_text += "<td colspan='" + colspan + "' rowspan='" + rowspan + "' valign='top'>" + cellContent + "</td>";
                }
            });
            tab_text += "</tr>";
        });
    }

    tab_text += "</table>";
    tab_text = tab_text.replace(/<A[^>]*>|<\/A>/g, "");
    tab_text = tab_text.replace(/<img[^>]*>/gi, "");
    tab_text = tab_text.replace(/<input[^>]*>|<\/input>/gi, "");

    var blob = new Blob([tab_text], { type: "application/vnd.ms-excel" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = 'DataTableExport.xls';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

//for Xlxs
//This requires plugin
//<script src="https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js"></script>
function fnExcelParmReportXlsx(TableToExport, profile, parameter, woksheetname = "New Workshet") {
    var $table = $(TableToExport);
    var workbook = XLSX.utils.book_new();
    var worksheet = XLSX.utils.aoa_to_sheet([]);

    XLSX.utils.sheet_add_aoa(worksheet, [[profile]], { origin: "A1" });
    XLSX.utils.sheet_add_aoa(worksheet, [[parameter]], { origin: "A2" });

    // Merge cells from A1 to AJ1 for the profile
    worksheet['!merges'] = worksheet['!merges'] || [];
    worksheet['!merges'].push({ s: { r: 0, c: 0 }, e: { r: 0, c: 35 } });
    worksheet['!merges'].push({ s: { r: 1, c: 0 }, e: { r: 1, c: 35 } });
    var data = [];
    var spanMap = {};
    var columnCounts = {};
    $table.find('tr').each(function (rowIndex) {
        var row = [];
        $(this).find('th, td').each(function () {
            var $cell = $(this);
            if ($cell.css('display') !== 'none' && !$cell.attr('hidden') && !$cell.hasClass("excelDisable")) {
                var colspan = parseInt($cell.attr('colspan')) || 1;
                var rowspan = parseInt($cell.attr('rowspan')) || 1;
                var cellContent = $cell.text().trim();
                // Find the next available index for this row
                var colIndex = row.length;
                while (spanMap[`${rowIndex}:${colIndex}`]) {
                    row.push("");
                    colIndex++;
                }
                row.push(cellContent);

                // Apply bold style if it's a <th> element
                if ($cell.is('th')) {
                    var cellRef = XLSX.utils.encode_cell({ r: rowIndex + 2, c: colIndex });

                    // Ensure the cell exists before applying the style
                    if (!worksheet[cellRef]) {
                        worksheet[cellRef] = {}; // Initialize the cell if it doesn't exist
                    }

                    worksheet[cellRef].s = {
                        font: { bold: true }
                    };
                }

                // Mark spanMap for rowspan and colspan
                for (var i = 0; i < rowspan; i++) {
                    for (var j = 0; j < colspan; j++) {
                        if (!(i === 0 && j === 0)) {
                            spanMap[`${rowIndex + i}:${colIndex + j}`] = true;
                        }
                    }
                }

                // Add empty cells for colspan
                for (var i = 1; i < colspan; i++) {
                    row.push("");
                }
            }
        });

        if (row.length > 0) {
            data.push(row);
        }
    });
    XLSX.utils.sheet_add_aoa(worksheet, data, { origin: "A3" });
    // Auto-adjust column widths based on content
    var colWidths = data.reduce((widths, row) => {
        row.forEach((cell, index) => {
            var width = cell ? cell.length : 10;
            widths[index] = Math.max(widths[index] || 0, width);
        });
        return widths;
    }, []);
    worksheet['!cols'] = colWidths.map(width => ({ wch: width + 2 }));

    // Append the worksheet to the workbook
    XLSX.utils.book_append_sheet(workbook, worksheet, "Sheet1");

    // Generate Blob and download the file
    var wbout = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    var blob = new Blob([wbout], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    if (woksheetname == null || woksheetname == undefined || woksheetname == '') {
        a.download = `datatableExport.xlsx`;

    } else {
        a.download = `${woksheetname}.xlsx`;
    }
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}





