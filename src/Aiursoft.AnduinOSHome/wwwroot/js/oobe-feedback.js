document.addEventListener("DOMContentLoaded", () => {
    const tabs = [...document.querySelectorAll("#feedback-tabs [role=tab]")];
    const select = tab => {
        tabs.forEach(item => {
            const selected = item === tab;
            item.setAttribute("aria-selected", String(selected));
            item.tabIndex = selected ? 0 : -1;
            item.classList.toggle("btn-primary", selected);
            item.classList.toggle("btn-outline-secondary", !selected);
            document.getElementById(item.getAttribute("aria-controls")).hidden = !selected;
        });
    };
    tabs.forEach((tab, index) => {
        tab.addEventListener("click", () => select(tab));
        tab.addEventListener("keydown", event => {
            let next;
            if (event.key === "ArrowRight") next = tabs[(index + 1) % tabs.length];
            if (event.key === "ArrowLeft") next = tabs[(index + tabs.length - 1) % tabs.length];
            if (event.key === "Home") next = tabs[0];
            if (event.key === "End") next = tabs[tabs.length - 1];
            if (next) {
                event.preventDefault();
                select(next);
                next.focus();
            }
        });
    });
});
