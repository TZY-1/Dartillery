// Dartboard JavaScript Interop
window.dartboardInterop = {
    /**
     * Converts screen coordinates to SVG normalized coordinates
     * @param {SVGElement} svgElement - The SVG element reference
     * @param {number} clientX - Mouse X position in screen coordinates
     * @param {number} clientY - Mouse Y position in screen coordinates
     * @returns {object} - Normalized coordinates {x, y}
     */
    getSvgCoordinates: function(svgElement, clientX, clientY) {
        const pt = svgElement.createSVGPoint();
        pt.x = clientX;
        pt.y = clientY;
        const svgPt = pt.matrixTransform(svgElement.getScreenCTM().inverse());
        return { x: svgPt.x, y: svgPt.y };
    }
};

/**
 * Plays an audio file
 * @param {string} audioPath - Path to the audio file (e.g., "/sounds/throw.mp3")
 */
window.playAudio = function(audioPath) {
    const audio = new Audio(audioPath);
    audio.volume = 0.3; // 30% volume to avoid being too loud
    audio.play().catch(err => {
        console.warn('Audio playback failed:', err);
    });
};
