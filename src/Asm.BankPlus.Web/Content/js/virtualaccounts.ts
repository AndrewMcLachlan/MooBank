module virtualaccounts {


    $(function () {
        setEventHandlers();
    });

    export function refresh() {
        $('#accountManagement').load('/virtualaccounts #accountManagement', function () {
            setEventHandlers();
        });
    }

    function setEventHandlers() {
        $('#add').click(function () {

            $.ajax({
                cache: false,
                type: "POST",
                url: '/virtualaccounts/newaccount',
                success: function (data) {
                    virtualaccounts.refresh();
                }
            });

        });

        $('[id^=close]').click(function () {

            if (!confirm("Are you sure you want to close this account?")) return;

            var $this = $(this);
            $.ajax({
                cache: false,
                type: "DELETE",
                url: '/virtualaccounts/deleteaccount/' + $this.attr("data-id"),
                success: function (data) {
                    virtualaccounts.refresh();
                }
            });
        });

        $('[id$=__Name]').change(function () {
            var $this = $(this);
            $.ajax({
                cache: false,
                type: "PATCH",
                url: '/virtualaccounts/updatename',///' + $this.attr("data-id"),
                data: { id: $this.attr("data-id"), name: $this.val() },
            });
        });

        $('[id$=__Description]').change(function () {
            var $this = $(this);
            $.ajax({
                cache: false,
                type: "PATCH",
                url: '/virtualaccounts/updatedescription/' + $this.attr("data-id"),
                data: { description: $this.val() },
            });
        });

        $('[name=DefaultAccount]').change(function () {
            var $this = $(this);
            $.ajax({
                cache: false,
                type: "PATCH",
                url: '/virtualaccounts/setdefaultaccount/' + $this.val(),
            });
        });
    }
}