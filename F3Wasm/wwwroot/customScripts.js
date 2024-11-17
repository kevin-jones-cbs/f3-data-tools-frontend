var allDates2, allDates3, allDates2Q1, allDates2Q2, allDates3Q1, allDates3Q2, allDates3Q3;

function setupDuplicateDates(dates2, dates3, dates2Q1, dates2Q2, dates3Q1, dates3Q2, dates3Q3) {
    allDates2 = dates2;
    allDates3 = dates3;
    allDates2Q1 = dates2Q1;
    allDates2Q2 = dates2Q2;
    allDates3Q1 = dates3Q1;
    allDates3Q2 = dates3Q2;
    allDates3Q3 = dates3Q3;

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

        //dates2Q1 should be blue and green
        if (allDates2Q1.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #007bff 50%, #28a745 50%)';
        }

        //dates2Q2 should be 2 shades of green
        if (allDates2Q2.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #28a745 50%, #116F26 50%)';
        }

        //dates3Q1 should be green and the colors from allDates2
        if (allDates3Q1.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #28a745 33.33%, #007bff 33.33%, #007bff 66.66%, #3013b7 66.66%)';
        }

        //dates3Q2 should be 2 shades of green and blue
        if (allDates3Q2.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #28a745 33.33%, #116F26 33.33%, #116F26 66.66%, #007bff 66.66%)';
        }

        //dates3Q3 should be 3 shades of green
        if (allDates3Q3.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #28a745 33.33%, #116F26 33.33%, #116F26 66.66%, #0d4f1d 66.66%)';
        }
    });
}

// Watch for changes so that we can apply styles to new elements as calendar changes
var observer = new MutationObserver(function(mutationsList, observer) {
    applyStylesBasedOnAriaLabel();
});

observer.observe(document, { childList: true, subtree: true });