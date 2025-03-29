import * as THREE from 'three';
import * as CANNON from 'https://cdn.jsdelivr.net/npm/cannon-es/dist/cannon-es.js';

import { camera } from './camera.js';
import { cameraBody } from './camera.js';
import { groundData } from './main.js';
import { center } from './main.js';

//Void Body
export const voidBody = new CANNON.Body({
    mass: 1,
    position: new CANNON.Vec3(0, 0, 0),
    shape: new CANNON.Plane()
});

export function initVoid() {
    //Respawn
        cameraBody.velocity.set(0, 0, 0);
    //

    const groundLimits = groundData.groundLimits;
    const maxHeight = -60;
    
    const respawnPos = {
        x: center.x,
        y: center.y,
        z: center.z,
    }
    
    function checkCameraVoid () {
        const camX = cameraBody.position.x;
        const camY = cameraBody.position.y;
        const camZ = cameraBody.position.z;
    
        const inAir = camX < groundLimits.minX || camX > groundLimits.maxX ||
        camZ < groundLimits.minZ || camZ > groundLimits.maxZ;
    
        if(inAir) {
            cameraBody.velocity.y = -15;
            const voidGravity = new CANNON.Vec3(0, 20, 0);
            cameraBody.applyForce(voidGravity, cameraBody.position);
    
            if(camY < maxHeight) {
                camera.position.set(respawnPos.x, respawnPos.y, respawnPos.z);
            }
        }
    }

    checkCameraVoid();
    voidBody.quaternion.setFromEuler(-Math.PI / 2, 0, 0);
}