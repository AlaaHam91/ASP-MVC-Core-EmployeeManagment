function confirmdelete(uniqueId,isTrue) {
    var deleteSpan = 'deleteSpan_' + uniqueId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isTrue) {
        $('#' + confirmDeleteSpan).show();
        $('#' + deleteSpan).hide();
    }
    else
    {
        $('#' + confirmDeleteSpan).hide();
        $('#' + deleteSpan).show();
    }
}