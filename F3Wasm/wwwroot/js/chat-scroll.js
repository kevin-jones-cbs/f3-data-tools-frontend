export function attach(root) {
    let following = true;
    let frame;
    const onScroll = () => {
        following = document.documentElement.scrollHeight - window.scrollY - window.innerHeight < 180;
    };
    const follow = (force = false) => {
        if (force) following = true;
        if (!following) return;
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            root.scrollIntoView({ block: 'end', behavior: 'instant' });
        });
    };
    const observer = new ResizeObserver(() => follow());
    observer.observe(root);
    window.addEventListener('scroll', onScroll, { passive: true });
    root.chatScroll = { follow, dispose: () => {
        observer.disconnect(); window.removeEventListener('scroll', onScroll); cancelAnimationFrame(frame);
    }};
}
export function follow(root, force) { root.chatScroll?.follow(force); }
export function detach(root) { root.chatScroll?.dispose(); delete root.chatScroll; }
