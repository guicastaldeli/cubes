import * as THREE from 'three';
import * as CANNON from 'https://cdn.jsdelivr.net/npm/cannon-es/dist/cannon-es.js';

import { camera } from './camera.js';
import { cameraBody } from './camera.js';
import { initFPCamera } from './camera.js';
import { initCameraControls } from './camera.js';
import { updateCamera } from './camera.js';

import { createCrosshair } from './crosshair.js';

import { createCubes } from '../cubes/cubes.js';
import { createGround } from '../ground/ground.js';

import { initVoid } from './void.js';
import { voidBody } from './void.js';

import { createParticles } from './particles.js';

//Configs
export const scene = new THREE.Scene();

//Skybox
const skyboxColor = 'rgb(211, 211, 211)';
scene.background = new THREE.Color(skyboxColor);

//Fog
const fogColor = 'rgb(211, 211, 211)';
scene.fog = new THREE.Fog(fogColor, 0, 25);

const renderer = new THREE.WebGLRenderer();
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setPixelRatio(window.devicePixelRatio);

window.addEventListener('resize', () => {
    renderer.setSize(window.innerWidth, window.innerHeight);
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
});

document.body.appendChild(renderer.domElement);

//Crosshair
createCrosshair(camera);
scene.add(camera);

let frameCount = 0;
let fps = 0;
let lastFrameTime = performance.now();
let fpsUpdateTime = performance.now();
const maxFPS = 120;

//Lightning
    //Directional Light
    const lightColor = 'rgb(255, 255, 255)';
    const directionalLight = new THREE.DirectionalLight(lightColor, 3);

    directionalLight.position.x = 50;
    directionalLight.position.y = 100;
    directionalLight.position.z = 80;

    const target = new THREE.Object3D();
    target.position.set(10, -20, 0);

    scene.add(target);

    directionalLight.shadow.camera.left = -200;
    directionalLight.shadow.camera.right = 200;
    directionalLight.shadow.camera.top = 100;
    directionalLight.shadow.camera.bottom = -150;

    directionalLight.shadow.bias = -0.0001;
    directionalLight.target = target;
    directionalLight.castShadow = true;

    directionalLight.shadow.mapSize.width = 4096;
    directionalLight.shadow.mapSize.height = 4096;

    scene.add(directionalLight);

    //Ambient Light
    const ambientLightColor = 'rgb(73, 73, 73)';
    const ambientLight = new THREE.AmbientLight(ambientLightColor);
    scene.add(ambientLight);

    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
//

//World
export const world = new CANNON.World();
world.gravity.set(0, -1, 0);

//Audio
    const listener = new THREE.AudioListener();

    const audioLoader = new THREE.AudioLoader();
    let ambienceSound;

    audioLoader.load('../../assets/audio/game/ambience.ogg', (buffer) => {
        ambienceSound = new THREE.Audio(listener);
        ambienceSound.setBuffer(buffer);
        ambienceSound.setLoop(true);
        ambienceSound.setVolume(0.15);
    });
//

//Render
    //Particles
        const renderParticles = createParticles();
    //

    //Ground
        export const groundData = createGround(world);
        const renderGround = groundData.renderGround;
        export const center = groundData.center;
                
        camera.position.set(center.x, center.y + 2, center.z);
        cameraBody.position.copy(camera.position);
        
        renderGround.receiveShadow = true;
        scene.add(renderGround);
    //

    const renderCubes = createCubes();

    renderCubes.forEach(({ cube }) => {
        cube.castShadow = true;
        cube.receiveShadow = true;

        scene.add(cube);
    });
//

//Gravity & Col
    //Void
        world.addBody(voidBody);
    //

    //Camera
        world.addBody(cameraBody);
    //

    //Ground
        export const groundBody = groundData.groundBody;
        world.addBody(groundBody);
    //
//

//FPS Display
const fpsText = document.createElement('p');
fpsText.id = 'fps';

document.addEventListener('keydown', (e) => {
    if(e.key === 'F3') {
        if(document.body.contains(fpsText)) {
            document.body.removeChild(fpsText);
        } else {
            document.body.appendChild(fpsText);
        }
    }
});

//Transition
document.addEventListener('DOMContentLoaded', () => {
    const fadeIn = localStorage.getItem('fadeIn') === 'true';

    if(fadeIn) {
        document.body.classList.add('fade-in');
        localStorage.removeItem('fadeIn');
    } else {
        document.body.style.opacity = 1;
    }
});

//Paused
    export const gameState = {
        paused: false,
        wasPointerLocked: false,
    }

    export let isPointer = false;

    async function requestPointer() {
        if(isPointer || document.pointerLockElement) return;

        try {
            await document.body.requestPointerLock();
        } catch(err) {
            if(!err.message.inclides("exited")) {
                setTimeout(() => requestPointer(), 100);
            }
        } finally {
            isPointer = false;
        }
    }

    const pauseMenuContainer = `
        <div class="pause-menu" style="display: none">
            <div id="overflow"></div>

            <div id="pause-menu-el">
                <p id="p-title">PAUSED</p>

                <div id="pause-menu-opts">
                    <button id="btn-back">Back to Game</button>
                    <button id="btn-return">Return to Menu</button>
                </div>
            </div>
        </div>
    `;
    document.body.insertAdjacentHTML('beforeend', pauseMenuContainer);
    const pauseMenu = document.querySelector('.pause-menu');

    export function togglePause() {
        gameState.paused = !gameState.paused;

        if(gameState.paused) {
            pauseMenu.style.display = 'block';
            gameState.wasPointerLocked = document.pointerLockElement === document.body;

            if(gameState.wasPointerLocked) {
                setTimeout(() => {
                    document.exitPointerLock();
                }, 50);
            }

            if(ambienceSound && ambienceSound.isPlaying) {
                ambienceSound.pause();
            }
        } else {
            pauseMenu.style.display = 'none';
            if(ambienceSound && !ambienceSound.isPlaying) {
                ambienceSound.play();
            }

            if(gameState.wasPointerLocked) {
                requestAnimationFrame(() => {
                    requestPointer();
                });
                gameState.wasPointerLocked = false;
            }
        }
    }

    //Back
    const btnBack = document.getElementById('btn-back');

    btnBack.addEventListener('click', () => {
        pauseMenu.style.display = 'none';
        gameState.paused = false;
    });

    //Return
    const btnReturn = document.getElementById('btn-return');

    async function redirect() {
        if(btnReturn) {
            localStorage.setItem('fadeOut', 'true');
            document.body.classList.add('fade-out');

            await new Promise(res => setTimeout(res, 500));
            window.location.href = '../../menu/index.html';
        }
    }

    btnReturn.addEventListener('click', redirect);
//

//Audio Buttons
const audioContainer = `
    <audio src="../../assets/audio/menu/select-1.ogg" id="click-sound"></audio>
`;
document.body.insertAdjacentHTML('beforeend', audioContainer);

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

//Render
function render() {
    if(gameState.paused) {
        requestAnimationFrame(render);
        return;
    }

    const currentTime = performance.now();
    const deltaTime = currentTime - lastFrameTime;

    if(deltaTime < 1000 / maxFPS) {
        requestAnimationFrame(render);
        return;
    }
    
    lastFrameTime = currentTime;
    frameCount++;

    if(currentTime - fpsUpdateTime >= 1000) {
        fps = frameCount;
        frameCount = 0;
        fpsUpdateTime = currentTime;

        fpsText.textContent = `FPS: ${fps}`;
    }

    world.step(1 / 60, deltaTime / 1000, 3);

    renderCubes.forEach(cube => {
        cube.cube.position.copy(cube.cubesBody.position);
        cube.cube.quaternion.copy(cube.cubesBody.quaternion);
    });

    //Audio
    if(ambienceSound && !ambienceSound.isPlaying) {
        ambienceSound.play();
    }

    camera.position.copy(cameraBody.position);

    renderParticles();
    initVoid();
    updateCamera();

    renderer.render(scene, camera);
    requestAnimationFrame(render);
}

function init() {
    initFPCamera(renderer);
    initCameraControls();
    render();
}

init();
activateAudio();
