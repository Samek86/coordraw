import { memo } from 'react';
import { Handle, Position, NodeProps } from 'reactflow';

export const DiagramNode = memo(({ data }: NodeProps) => {
  const isGroup = data.entityType === 'group';
  const isIcon = data.entityType === 'icon';
  
  return (
    <div
      className={`diagram-node ${isGroup ? 'group' : ''} ${isIcon ? 'icon' : ''}`}
      style={{
        background: data.color || (isGroup ? 'rgba(52, 152, 219, 0.05)' : 'white'),
        border: `2px ${isGroup ? 'dashed' : 'solid'} ${data.color || '#3498db'}`,
        borderRadius: '8px',
        padding: '10px',
        minWidth: data.width || 150,
        minHeight: data.height || 60,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: isGroup ? '16px' : '14px',
        fontWeight: isGroup ? 600 : 400
      }}
    >
      <Handle type="target" position={Position.Top} />
      <div>{data.label}</div>
      <Handle type="source" position={Position.Bottom} />
    </div>
  );
});

DiagramNode.displayName = 'DiagramNode';
