const { ipcMain } = require('electron')
const { app, BrowserWindow } = require('electron/main')
const path = require('node:path')

function createWindow () {
  const win = new BrowserWindow({
    width: 804,
    height: 486,
    icon: __dirname + '../../assets/img/ico.png',
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.js')
    }
  })

  win.loadFile('./menu/index.html')

  //Full screen
  ipcMain.on('toggle-fullscreen', () => {
    const isFullscreen = win.isFullScreen();
    win.setFullScreen(!isFullscreen);
  });

  //Quit
  ipcMain.on('quit-game', () => {
    app.quit();
  });

}

app.whenReady().then(() => {
  createWindow()

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow()
    }
  })
})

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit()
  }
})