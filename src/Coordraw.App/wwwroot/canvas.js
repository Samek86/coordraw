// Coordraw Canvas - WebView2 Embedded
// Pixel-perfect drag and resize

let diagram = { entities: [], connections: [] };
let selectedNode = null;
let isDragging = false;
let isResizing = false;
let dragStartX = 0;
let dragStartY = 0;
let nodeStartX = 0;
let nodeStartY = 0;

const canvas = document.getElementById('canvas');

function loadDiagram(data) {
    diagram = data;
    render();
    notifyHost({ action: 'ready' });
}

function render() {
    canvas.innerHTML = '';
    
    // Render entities
    diagram.entities.forEach(entity => {
        const node = createNode(entity);
        canvas.appendChild(node);
    });
    
    // Render connections
    diagram.connections.forEach(conn => {
        const connection = createConnection(conn);
        canvas.appendChild(connection);
    });
}

function createNode(entity) {
    const node = document.createElement('div');
    node.className = 'node';
    node.id = entity.id;
    node.style.left = entity.x + 'px';
    node.style.top = entity.y + 'px';
    
    if ('width' in entity) {
        node.style.width = entity.width + 'px';
        node.style.height = entity.height + 'px';
    }
    
    if (entity.type === 'group') {
        node.classList.add('group');
    } else if (entity.type === 'icon') {
        node.classList.add('icon');
    }
    
    if (entity.color) {
        node.style.borderColor = entity.color;
    }
    
    const label = entity.texts && entity.texts.length > 0 ? entity.texts[0].text : entity.id;
    node.textContent = label;
    
    // Add resize handle
    const resizeHandle = document.createElement('div');
    resizeHandle.className = 'resize-handle';
    node.appendChild(resizeHandle);
    
    // Event listeners
    node.addEventListener('mousedown', (e) => onNodeMouseDown(e, node, entity));
    resizeHandle.addEventListener('mousedown', (e) => onResizeStart(e, node, entity));
    
    return node;
}

function createConnection(conn) {
    const fromNode = document.getElementById(conn.from);
    const toNode = document.getElementById(conn.to);
    
    if (!fromNode || !toNode) return document.createElement('div');
    
    const fromRect = fromNode.getBoundingClientRect();
    const toRect = toNode.getBoundingClientRect();
    const canvasRect = canvas.getBoundingClientRect();
    
    const fromX = fromRect.left - canvasRect.left + fromRect.width / 2;
    const fromY = fromRect.top - canvasRect.top + fromRect.height / 2;
    const toX = toRect.left - canvasRect.left + toRect.width / 2;
    const toY = toRect.top - canvasRect.top + toRect.height / 2;
    
    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    svg.classList.add('connection');
    if (conn.lineStyle === 'dashed') svg.classList.add('dashed');
    svg.style.left = '0';
    svg.style.top = '0';
    svg.style.width = '100%';
    svg.style.height = '100%';
    
    const line = document.createElementNS('http://www.w3.org/2000/svg', 'line');
    line.setAttribute('x1', fromX);
    line.setAttribute('y1', fromY);
    line.setAttribute('x2', toX);
    line.setAttribute('y2', toY);
    svg.appendChild(line);
    
    if (conn.label) {
        const text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
        text.setAttribute('x', (fromX + toX) / 2);
        text.setAttribute('y', (fromY + toY) / 2 - 5);
        text.setAttribute('text-anchor', 'middle');
        text.textContent = conn.label;
        svg.appendChild(text);
    }
    
    return svg;
}

function onNodeMouseDown(e, node, entity) {
    if (e.target.classList.contains('resize-handle')) return;
    
    e.stopPropagation();
    selectNode(node);
    
    isDragging = true;
    dragStartX = e.clientX;
    dragStartY = e.clientY;
    nodeStartX = entity.x;
    nodeStartY = entity.y;
}

function onResizeStart(e, node, entity) {
    e.stopPropagation();
    isResizing = true;
    selectedNode = { node, entity };
    dragStartX = e.clientX;
    dragStartY = e.clientY;
    nodeStartX = entity.width || 150;
    nodeStartY = entity.height || 60;
}

function selectNode(node) {
    document.querySelectorAll('.node').forEach(n => n.classList.remove('selected'));
    node.classList.add('selected');
    selectedNode = { node, entity: diagram.entities.find(e => e.id === node.id) };
}

document.addEventListener('mousemove', (e) => {
    if (isDragging && selectedNode) {
        const dx = e.clientX - dragStartX;
        const dy = e.clientY - dragStartY;
        
        const newX = nodeStartX + dx;
        const newY = nodeStartY + dy;
        
        selectedNode.node.style.left = newX + 'px';
        selectedNode.node.style.top = newY + 'px';
        selectedNode.entity.x = newX;
        selectedNode.entity.y = newY;
        
        // Redraw connections
        renderConnections();
    } else if (isResizing && selectedNode) {
        const dx = e.clientX - dragStartX;
        const dy = e.clientY - dragStartY;
        
        const newWidth = Math.max(80, nodeStartX + dx);
        const newHeight = Math.max(40, nodeStartY + dy);
        
        selectedNode.node.style.width = newWidth + 'px';
        selectedNode.node.style.height = newHeight + 'px';
        
        if ('width' in selectedNode.entity) {
            selectedNode.entity.width = newWidth;
            selectedNode.entity.height = newHeight;
        }
    }
});

document.addEventListener('mouseup', () => {
    if (isDragging || isResizing) {
        // Notify host of changes
        notifyHost({ action: 'save', data: diagram });
    }
    isDragging = false;
    isResizing = false;
});

function renderConnections() {
    document.querySelectorAll('.connection').forEach(c => c.remove());
    diagram.connections.forEach(conn => {
        const connection = createConnection(conn);
        canvas.appendChild(connection);
    });
}

function notifyHost(message) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage(message);
    }
}

// Deselect on canvas click
canvas.addEventListener('mousedown', (e) => {
    if (e.target === canvas) {
        document.querySelectorAll('.node').forEach(n => n.classList.remove('selected'));
        selectedNode = null;
    }
});

// Initial ready notification
if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage({ action: 'ready' });
}
