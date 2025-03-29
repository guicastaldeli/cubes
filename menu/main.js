function startGame() {
    const btnStart = document.getElementById('btn-start');

    async function redirect() {
        if(btnStart) {
            localStorage.setItem('fadeIn', 'true');
            
            const canvas = document.getElementById('canvas-menu');
            const menu = document.querySelector('.menu');

            if(canvas) canvas.classList.add('fade-out');
            if(menu) menu.classList.add('fade-out');

            await new Promise(res => setTimeout(res, 500));

            window.location.href = '../main/main/index.html';
        }
    }

    btnStart.addEventListener('click', redirect);
}

//Quit or Download Btn
    const btnQuit = document.getElementById('btn-quit');

    function quitDownloadGame() {
        if(!btnQuit) return;

        const isElectron = (
            (window?.process?.versions?.electron) ||
            (navigator.userAgent.toLowerCase().includes('electron')) ||
            (typeof require !== 'undefined' && require('electron'))
        );

        if(isElectron) {
            btnQuit.textContent = 'Quit';
            
            btnQuit.onclick = () => {
                if(typeof require !== 'undefined') {
                    try {
                        const { ipcRenderer } = require('electron');
                        ipcRenderer.send('quit-game');
                    } catch(e) {
                        console.error(e);
                        window.close();
                    }
                } else {
                    window.close();
                }
            }
        } else {
            btnQuit.textContent = 'Download the Game!';
            btnQuit.classList.add('btn-download');

            btnQuit.onclick = () => {
                window.open('https://google.com', '_blank');
            }
        } 
    }
//

function activateAudio() {
    const buttons = document.querySelectorAll('button');

    //Click
    const clickSound = document.getElementById('click-sound');

    if(!clickSound) return;

    clickSound.volume = 0.3;

    buttons.forEach(button => {
        button.addEventListener('click', () => {
            clickSound.currentTime = 0;
            clickSound.play().catch(e => console.log(e));
        });
    });
}

//Transition
document.addEventListener('DOMContentLoaded', () => {
    const fadeOut = localStorage.getItem('fadeOut') === 'true';

    if(fadeOut) {
        localStorage.removeItem('fadeOut');
        document.body.classList.add('fade-in');
    } else {
        document.body.style.opacity = '1';
    }
})

startGame();
quitDownloadGame();
activateAudio();