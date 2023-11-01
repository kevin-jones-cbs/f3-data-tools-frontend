var allDates2, allDates3;

function setupDuplicateDates(dates2, dates3) {
    allDates2 = dates2;
    allDates3 = dates3;
    applyStylesBasedOnAriaLabel();
}

function applyStylesBasedOnAriaLabel() {
    if (!allDates2 || !allDates3) {
        return;
    }

    var elements = document.querySelectorAll('[aria-label]');
    elements.forEach(function (element) {
        var ariaLabel = element.getAttribute('aria-label');
        if (allDates2.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #007bff 50%, #3013b7 50%)'; 
        }

        if (allDates3.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #007bff 33.33%, #3013b7 33.33%, #3013b7 66.66%, #7e80f9 66.66%)';
        }
    });
}

// Watch for changes so that we can apply styles to new elements as calendar changes
var observer = new MutationObserver(function(mutationsList, observer) {
    applyStylesBasedOnAriaLabel();
});

observer.observe(document, { childList: true, subtree: true });