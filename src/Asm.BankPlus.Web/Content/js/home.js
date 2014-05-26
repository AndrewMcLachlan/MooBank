// Module
var home;
(function (home) {
    $(function () {
    });

    function updateBalance(id) {
        display(id, true);
    }
    home.updateBalance = updateBalance;

    function save(id) {
        var currentBalanceDisplay = $('#currentBalanceDisplay_' + id);
        var currentBalanceEdit = $('#currentBalanceEdit_' + id);
        var availableBalanceDisplay = $('#availableBalanceDisplay_' + id);
        var availableBalanceEdit = $('#availableBalanceEdit_' + id);

        var currentBalance = parseFloat(currentBalanceEdit.val());
        var availableBalance = parseFloat(availableBalanceEdit.val());

        $.ajax({
            url: 'accounts/updatebalance/' + id,
            type: 'PATCH',
            cache: false,
            data: {
                currentBalance: currentBalance,
                availableBalance: availableBalance
            },
            success: function () {
                $('#virtualAccounts').load('/home/index #virtualAccounts', function () {
                    var currentBalanceChar = currentBalance < 0 ? 'D' : 'C';

                    currentBalanceDisplay.attr('class', currentBalance < 0 ? 'negative' : '');
                    currentBalanceDisplay.text(currentBalance.toFixed(2) + ' ' + currentBalanceChar + 'R');

                    var availableBalanceChar = availableBalance < 0 ? 'D' : 'C';

                    availableBalanceDisplay.attr('class', availableBalance < 0 ? 'negative' : '');
                    availableBalanceDisplay.text(availableBalance.toFixed(2) + ' ' + availableBalanceChar + 'R');

                    display(id, false);
                });
            },
            error: function () {
                home.cancel(id);
            }
        });
    }
    home.save = save;

    function cancel(id) {
        display(id, false);

        var currentBalanceDisplay = $('#currentBalanceDisplay_' + id);
        var currentBalanceEdit = $('#currentBalanceEdit_' + id);
        var availableBalanceDisplay = $('#availableBalanceDisplay_' + id);
        var availableBalanceEdit = $('#availableBalanceEdit_' + id);

        var currentBalance = currentBalanceDisplay.text();
        var availableBalance = availableBalanceDisplay.text();

        currentBalanceEdit.val(currentBalance.substring(0, currentBalance.length - 3));
        availableBalanceEdit.val(availableBalance.substring(0, availableBalance.length - 3));
    }
    home.cancel = cancel;

    function display(id, display) {
        var currentBalanceDisplay = $('#currentBalanceDisplay_' + id);
        var currentBalanceEdit = $('#currentBalanceEdit_' + id);

        var availableBalanceDisplay = $('#availableBalanceDisplay_' + id);
        var availableBalanceEdit = $('#availableBalanceEdit_' + id);

        var cancelButton = $('#cancel_' + id);
        var saveButton = $('#save_' + id);

        currentBalanceDisplay.toggle(!display);
        availableBalanceDisplay.toggle(!display);

        currentBalanceEdit.toggle(display);
        availableBalanceEdit.toggle(display);

        cancelButton.toggle(display);
        saveButton.toggle(display);
    }
})(home || (home = {}));
//# sourceMappingURL=home.js.map
