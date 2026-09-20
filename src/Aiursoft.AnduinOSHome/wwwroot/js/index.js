//=====================================
//         The Image Gallery
//=====================================
document.addEventListener('DOMContentLoaded', (() => {
    document.querySelectorAll('img[gallery]').forEach(x => {
        x.addEventListener('click', function () {
            igl_show(this)
        });
    });
}));

function igl_show(img) {
    var iglmodal = document.getElementById('iglmodal');
    var iglmodalImg = document.getElementById('iglmodal-img');
    iglmodal.style.display = 'flex';
    iglmodalImg.src = img.src.replace('_comp', '').replace('.webp', '.png');
    iglmodalImg.onclick = function (event) {
        event.stopPropagation();
    };
}

function igl_hide() {
    document.getElementById('iglmodal').style.display = 'none';
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('iglmodal')?.addEventListener('click', igl_hide);
    document.querySelectorAll('[data-requirement-panel]').forEach(button => {
        button.addEventListener('click', () => {
            document.querySelectorAll('[data-requirement-panel]').forEach(tab => {
                const active = tab === button;
                tab.classList.toggle('active', active);
                document.getElementById(tab.dataset.requirementPanel)?.classList.toggle('show', active);
            });
        });
    });
});
