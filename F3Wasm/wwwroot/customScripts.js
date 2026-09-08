var allDates2, allDates3, allDates2Q1, allDates2Q2, allDates3Q1, allDates3Q2, allDates3Q3, allDatesQSource;

function setupDuplicateDates(dates2, dates3, dates2Q1, dates2Q2, dates3Q1, dates3Q2, dates3Q3, datesQSource) {
    allDates2 = dates2;
    allDates3 = dates3;
    allDates2Q1 = dates2Q1;
    allDates2Q2 = dates2Q2;
    allDates3Q1 = dates3Q1;
    allDates3Q2 = dates3Q2;
    allDates3Q3 = dates3Q3;
    allDatesQSource = datesQSource;

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
            element.style.background = 'linear-gradient(to right, #3478f6 50%, #1f4fb8 50%)'; 
        }

        if (allDates3.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #3478f6 33.33%, #1f4fb8 33.33%, #1f4fb8 66.66%, #8db1fb 66.66%)';
        }

        //dates2Q1 should be blue and green
        if (allDates2Q1.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #25a667 50%, #3478f6 50%)';
        }

        //dates2Q2 should be 2 shades of green
        if (allDates2Q2.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #25a667 50%, #17804d 50%)';
        }

        //dates3Q1 should be green and the colors from allDates2
        if (allDates3Q1.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #25a667 33.33%, #3478f6 33.33%, #3478f6 66.66%, #1f4fb8 66.66%)';
        }

        //dates3Q2 should be 2 shades of green and blue
        if (allDates3Q2.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #25a667 33.33%, #17804d 33.33%, #17804d 66.66%, #3478f6 66.66%)';
        }

        //dates3Q3 should be 3 shades of green
        if (allDates3Q3.indexOf(ariaLabel) > -1) {
            element.style.background = 'linear-gradient(to right, #25a667 33.33%, #17804d 33.33%, #17804d 66.66%, #0f5c36 66.66%)';
        }

        //datesQSource should just change the font color
        if (allDatesQSource.indexOf(ariaLabel) > -1) {
            element.style.fontWeight = 'bold';
            element.style.color = '#ffd6f2';
            element.style.textDecoration = 'underline';
        }
    });
}

// Watch for changes so that we can apply styles to new elements as calendar changes
var observer = new MutationObserver(function(mutationsList, observer) {
    applyStylesBasedOnAriaLabel();
});

observer.observe(document, { childList: true, subtree: true });