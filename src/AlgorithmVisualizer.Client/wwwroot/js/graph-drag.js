export function beginDrag(svg, pointerId) {
    if (!svg) {
        throw new Error("Graph SVG element is unavailable.");
    }

    svg.setPointerCapture(pointerId);
}

export function endDrag(svg, pointerId) {
    if (svg && svg.hasPointerCapture(pointerId)) {
        svg.releasePointerCapture(pointerId);
    }
}

export function measureViewport(container) {
    if (!container) {
        return { width: 0, height: 0 };
    }

    return {
        width: container.clientWidth,
        height: container.clientHeight
    };
}

export function adjustScroll(container, deltaX, deltaY) {
    if (!container) {
        return;
    }

    if (Number.isFinite(deltaX) && deltaX !== 0) {
        container.scrollLeft += deltaX;
    }

    if (Number.isFinite(deltaY) && deltaY !== 0) {
        container.scrollTop += deltaY;
    }
}

export function resetScroll(container) {
    if (!container) {
        return;
    }

    container.scrollTo({ left: 0, top: 0, behavior: "auto" });
}
