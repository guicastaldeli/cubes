const { ipcRenderer } = require("electron")

window.addEventListener('DOMContentLoaded', () => {
  const replaceText = (selector, text) => {
    const element = document.getElementById(selector)
    if (element) element.innerText = text
  }
  
  for (const type of ['chrome', 'node', 'electron']) {
    replaceText(`${type}-version`, process.versions[type])
  }
})


//Fullscreen
window.addEventListener('keydown', (e) => {
  if(e.key === 'F9') {
    ipcRenderer.send('toggle-fullscreen');
  }
})