import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react';

export interface SignaturePadHandle {
  getDataUrl: () => string;
  clear: () => void;
  isEmpty: () => boolean;
}

interface Props {
  width?: number;
  height?: number;
}

const SignaturePad = forwardRef<SignaturePadHandle, Props>(({ width = 480, height = 160 }, ref) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const drawingRef = useRef(false);
  const [empty, setEmpty] = useState(true);

  useEffect(() => {
    const canvas = canvasRef.current!;
    const dpr = window.devicePixelRatio || 1;
    canvas.width = width * dpr;
    canvas.height = height * dpr;
    canvas.style.width = `${width}px`;
    canvas.style.height = `${height}px`;
    const ctx = canvas.getContext('2d')!;
    ctx.scale(dpr, dpr);
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.strokeStyle = '#111';
  }, [width, height]);

  function pos(e: React.PointerEvent<HTMLCanvasElement>): [number, number] {
    const rect = canvasRef.current!.getBoundingClientRect();
    return [e.clientX - rect.left, e.clientY - rect.top];
  }

  function onDown(e: React.PointerEvent<HTMLCanvasElement>) {
    e.preventDefault();
    canvasRef.current!.setPointerCapture(e.pointerId);
    const ctx = canvasRef.current!.getContext('2d')!;
    const [x, y] = pos(e);
    ctx.beginPath();
    ctx.moveTo(x, y);
    drawingRef.current = true;
  }

  function onMove(e: React.PointerEvent<HTMLCanvasElement>) {
    if (!drawingRef.current) return;
    const ctx = canvasRef.current!.getContext('2d')!;
    const [x, y] = pos(e);
    ctx.lineTo(x, y);
    ctx.stroke();
    if (empty) setEmpty(false);
  }

  function onUp(e: React.PointerEvent<HTMLCanvasElement>) {
    drawingRef.current = false;
    canvasRef.current!.releasePointerCapture(e.pointerId);
  }

  useImperativeHandle(ref, () => ({
    getDataUrl: () => canvasRef.current!.toDataURL('image/png'),
    clear: () => {
      const canvas = canvasRef.current!;
      const ctx = canvas.getContext('2d')!;
      ctx.save();
      ctx.setTransform(1, 0, 0, 1, 0, 0);
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.restore();
      setEmpty(true);
    },
    isEmpty: () => empty,
  }));

  return (
    <canvas
      ref={canvasRef}
      onPointerDown={onDown}
      onPointerMove={onMove}
      onPointerUp={onUp}
      onPointerCancel={onUp}
      style={{ border: '1px solid #ccc', borderRadius: 4, touchAction: 'none', background: '#fff' }}
    />
  );
});

export default SignaturePad;
