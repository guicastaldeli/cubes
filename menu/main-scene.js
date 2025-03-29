import * as THREE from 'three';
import { createMenuCubes } from './menu-cubes.js';

export const scene = new THREE.Scene();
export const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);

export const renderer = new THREE.WebGLRenderer({
    canvas: document.getElementById('canvas-menu'),
    alpha: true
});
renderer.setSize(window.innerWidth, window.innerHeight);

camera.rotation.z = THREE.MathUtils.degToRad(90);
camera.rotation.x = THREE.MathUtils.degToRad(90);

camera.position.x = 2;
camera.position.y = 5;
camera.position.z = 5;

//Skybox
const skyboxColor = 'rgb(211, 211, 211)';
scene.background = new THREE.Color(skyboxColor);

//Fog
const fogColor = 'rgb(211, 211, 211)';
scene.fog = new THREE.Fog(fogColor, 0, 40);

window.addEventListener('resize', () => {
    renderer.setSize(window.innerWidth, window.innerHeight);
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
});

//Lightning
    //Directional Light
        const lightColor = 'rgb(255, 255, 255)';
        const directionalLight = new THREE.DirectionalLight(lightColor, 3);

        directionalLight.position.x = 50;
        directionalLight.position.y = 100;
        directionalLight.position.z = 80;

        const target = new THREE.Object3D();
        target.position.set(10, 20, 0);

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
    //

    //Ambient Light
    const ambientLightColor = 'rgb(73, 73, 73)';
    const ambientLight = new THREE.AmbientLight(ambientLightColor);
    scene.add(ambientLight);
//

//Cubes
const renderCubes = createMenuCubes();
renderCubes.forEach(cube => {
    scene.add(cube);
});

let frameCount = 0;
let fps = 0;
let lastFrameTime = performance.now();
let fpsUpdateTime = performance.now();
const maxFPS = 60;

//Render
function render() {
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
    }

    renderer.render(scene, camera);
    requestAnimationFrame(render);
}

window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    renderer.setSize(window.innerWidth, window.innerHeight);
    camera.updateProjectionMatrix();
})

render();

