import * as THREE from 'three';
import * as CANNON from 'https://cdn.jsdelivr.net/npm/cannon-es/dist/cannon-es.js';
import { FirstPersonControls } from 'three/addons/controls/FirstPersonControls.js';

import { gameState, isPointer } from './main.js';
import { togglePause } from './main.js';

//Cameras
    export const camera = new THREE.PerspectiveCamera(115, window.innerWidth / window.innerHeight, 0.1, 1000);

    export const cameraControls = {
        MOVE_SPEED: 0.2,
        FLY_SPEED: 0.1,
        MOUSE_SENSV: 0.009,
        MAX_MOUSE_MOV: 20,
        flyMode: false,
        isPointerLocked: false,
        mouseMovX: 0,
        mouseMovY: 0,
        keyPressed: {},
        targetPos: new THREE.Vector3(camera.position.x, camera.position.y, camera.position.z),
    }
//

//Camera Body
    const playerSize = new CANNON.Vec3(1, 2, 1);
    const cameraShape = new CANNON.Box(playerSize);

    const GROUND_GROUP = 1;
    const PLAYER_GROUP = 2;

    export const cameraBody = new CANNON.Body({
        mass: 1,
        shape: cameraShape,
        position: new CANNON.Vec3(0, 0, 0),
        linearDamping: 0.1,
        fixedRotation: true
    });

    cameraBody.collisionFilterGroup = PLAYER_GROUP;
    cameraBody.collisionFilterMask = GROUND_GROUP;

    cameraBody.velocity.set(0, 0, 0);
//

//FP Controls
    let fpControls;

    export function initFPCamera(renderer) {
        fpControls = new FirstPersonControls(camera, renderer.domElement);
        fpControls.lookSpeed = 0.1;
        fpControls.movementSpeed = 0.5;
        fpControls.enabled = false;
    }
//

export function initCameraControls() {
    //Keys
        window.addEventListener('keydown', (e) => {
            cameraControls.keyPressed[e.key.toLowerCase()] = true;
        });

        window.addEventListener('keyup', (e) => {
            cameraControls.keyPressed[e.key.toLowerCase()] = false;
        });
    //

    //Mouse
        document.addEventListener('keydown', (e) => {
            if(e.key === 'Escape') {
                togglePause();
            } else {
                document.body.requestPointerLock();
            }
        });

        document.addEventListener('click', () => {
            if(!cameraControls.isPointerLocked && !gameState.paused) {
                document.body.requestPointerLock();
            }
        })

        document.addEventListener('pointerlockchange', () => {
            if(isPointer) return;
            
            cameraControls.isPointerLocked = document.pointerLockElement === document.body;
            if(!isPointer) fpControls.enabled = cameraControls.isPointerLocked && !gameState.paused;

            if(!cameraControls.isPointerLocked && !gameState.paused) {
                togglePause();
            }
        });

        document.addEventListener('mousemove', (e) => {
            if(cameraControls.isPointerLocked) {
                let movX = e.movementX || 0;
                let movY = e.movementY || 0;

                movX = Math.max(-cameraControls.MAX_MOUSE_MOV, Math.min(cameraControls.MAX_MOUSE_MOV, movX));
                movY = Math.max(-cameraControls.MAX_MOUSE_MOV, Math.min(cameraControls.MAX_MOUSE_MOV, movY));

                const euler = new THREE.Euler(0, 0, 0, 'YXZ');
                euler.setFromQuaternion(camera.quaternion);

                euler.y -= movX * cameraControls.MOUSE_SENSV;
                euler.x -= movY * cameraControls.MOUSE_SENSV;

                const maxVerticalAngle = Math.PI / 2 - 0.1;
                euler.x = Math.max(-maxVerticalAngle, Math.min(maxVerticalAngle, euler.x));

                camera.quaternion.setFromEuler(euler);
            }
        })
    //
}

//Audio
    const listener = new THREE.AudioListener();
    camera.add(listener);

    const audioLoader = new THREE.AudioLoader();
    let stepSound;
    
    audioLoader.load('/assets/audio/game/steps.ogg', (buffer) => {
        stepSound = new THREE.Audio(listener);
        stepSound.setBuffer(buffer);
        stepSound.setLoop(true);
        stepSound.setVolume(0);
    });
//

export function updateCamera() {
    const direction = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion);
    const forward = new THREE.Vector3(direction.x, 0, direction.z).normalize();
    const right = new THREE.Vector3(direction.z, 0, -direction.x).normalize();

    //Keys
    const speed = cameraControls.MOVE_SPEED;

    let moveX = 0;
    let moveZ = 0;

    if(cameraControls.keyPressed['w']) {
        moveZ -= 1;
    }
    if(cameraControls.keyPressed['s']) {
        moveZ += 1;
    }
    if(cameraControls.keyPressed['a']) {
        moveX -= 1;
    }
    if(cameraControls.keyPressed['d']) {
        moveX += 1;
    }

    const moveVector = new THREE.Vector3(moveX, 0, moveZ);
    const isMoving = moveVector.length() > 0;

    if(isMoving) {
        moveVector.normalize();

        const moveDirection = new THREE.Vector3();
        moveDirection.addScaledVector(forward, -moveZ);
        moveDirection.addScaledVector(right, -moveX);
        
        moveDirection.normalize();

        const targetPos = camera.position.clone().addScaledVector(moveDirection, speed);
        const walkHeight = Math.sin(Date.now() * 0.015) * 0.08;
        targetPos.y += walkHeight;

        camera.position.lerp(targetPos, speed);

        if(stepSound && !cameraControls.flyMode) {
            const stepIntensity = Math.abs(walkHeight) * 12.5;
            stepSound.setVolume(Math.min(0.5, stepIntensity * 0.4));
            stepSound.playbackRate = 0.8 + (stepIntensity * 0.4);

            if(walkHeight > 0.07 && !stepSound.isPlaying) {
                stepSound.play();
            }

            if(!stepSound.isPlaying) {
                stepSound.play();
            }
        }
    } else {
        if(stepSound && stepSound.isPlaying) {
            stepSound.setVolume(Math.max(0, stepSound.getVolume() - 0.05));
            if(stepSound.getVolume() <= 0.05) {
                stepSound.stop();
            }
        }
    }

    cameraBody.position.copy(camera.position);
}
