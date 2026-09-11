#!/usr/bin/env node

/**
 * Coordraw CLI
 * Compile and render diagrams from DSL
 */

import { readFile, writeFile, mkdir } from 'fs/promises';
import { dirname, resolve, join } from 'path';
import { Command } from 'commander';
import { parse } from '@coordraw/dsl';
import { compile } from './compiler.js';
import { execFile } from 'child_process';
import { promisify } from 'util';

const execFileAsync = promisify(execFile);

const program = new Command();

program
  .name('coordraw')
  .description('Coordraw - DSL to Eraser Diagrams compiler and renderer')
  .version('0.1.0');

program
  .command('compile')
  .description('Compile DSL to Eraser Diagrams JSON')
  .argument('<input>', 'Input .crd file')
  .option('-o, --output <file>', 'Output JSON file')
  .action(async (input: string, options: { output?: string }) => {
    try {
      const source = await readFile(input, 'utf-8');
      const parseResult = parse(source);

      if (parseResult.errors.length > 0) {
        console.error('Parse errors:');
        for (const error of parseResult.errors) {
          console.error(`  Line ${error.line}: ${error.message}`);
        }
        if (parseResult.errors.some(e => e.line === 0)) {
          process.exit(1);
        }
      }

      const eraserDiagram = compile(parseResult.diagram);
      const output = options.output || input.replace(/\.crd$/, '.json');
      
      await mkdir(dirname(output), { recursive: true });
      await writeFile(output, JSON.stringify(eraserDiagram, null, 2));
      
      console.log(`✓ Compiled ${input} → ${output}`);
    } catch (error) {
      console.error('Compilation failed:', error);
      process.exit(1);
    }
  });

program
  .command('render')
  .description('Compile and render DSL to PNG')
  .argument('<input>', 'Input .crd file')
  .option('-o, --output <file>', 'Output PNG file')
  .option('--fonts <file>', 'Fonts configuration JSON')
  .option('--chromium <path>', 'Path to Chromium executable')
  .action(async (input: string, options: { output?: string; fonts?: string; chromium?: string }) => {
    try {
      // Step 1: Compile to JSON
      const source = await readFile(input, 'utf-8');
      const parseResult = parse(source);

      if (parseResult.errors.length > 0) {
        console.error('Parse errors:');
        for (const error of parseResult.errors) {
          console.error(`  Line ${error.line}: ${error.message}`);
        }
      }

      const eraserDiagram = compile(parseResult.diagram);
      const tempJson = input.replace(/\.crd$/, '.temp.json');
      await writeFile(tempJson, JSON.stringify(eraserDiagram, null, 2));

      // Step 2: Render with eraser-diagrams-cli
      const output = options.output || input.replace(/\.crd$/, '.png');
      await mkdir(dirname(output), { recursive: true });

      const cliArgs = [tempJson, '--output', output];
      
      if (options.fonts) {
        cliArgs.push('--fonts', options.fonts);
      }
      
      if (options.chromium) {
        cliArgs.push('--chromium', options.chromium);
      }

      // Find eraser-diagrams-cli
      const cliPath = resolve(process.cwd(), 'node_modules/.bin/eraser-diagrams-cli');
      
      console.log(`Rendering ${input}...`);
      await execFileAsync(cliPath, cliArgs);
      
      console.log(`✓ Rendered ${input} → ${output}`);
    } catch (error) {
      console.error('Render failed:', error);
      process.exit(1);
    }
  });

program.parse();
