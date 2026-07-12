# Ostrich emulator

A simple Game Boy emulator written in C# using SDL2 and ImGUI. This project is currently in backburner development, feel free to have a poke around.
The current main focus has been on implementing the opcodes and making debugging possible, turning on the debugging feature via the menu will show
you what's getting loaded into the registers as the ROM loads, the display currently doesn't output yet.

### FAQs

 - ROM selection: If you're on Linux using some minimal VM you'll have to enter the path manually.

### Dev requirements

- Dotnet SDK 10
- libsdl2 & libglfw3

on UbuntuDebian using Wayland install the following packages:<br/>
`sudo apt install dotnet-sdk-10.0 libglfw3 libsdl2-2.0-0 libgl1 libegl1 libx11-6 libwayland-client0`

### Display

The display uses only integer scaling in relation to the emulators display buffer. This is so that the rendered image
better reflects the hardware being emulated.

Window Pipeline:
```
    Window
    ├─ Padding
    │   └─ Content Area
    │       └─ Integer-scaled emulator image
    └─ ImGui overlay
```

Padding has been added around the content area. This was present on original hardware and games would draw to the edge
of the screen with the knowledge that the padding was there.

### Project Structure

App (FrontEnd), it holds knowledge of Silk.NET, OpenGL, ImGui and is generally responsible for HCI tasks.

Emulation (Contracts), is an interface layer defining what should be exposed.

GameBoy (Core), implements the contracts and holds knowledge of the emulated hardware.

#### Reference Graph

Ostrich.App       ──▶  Ostrich.Emulation
Ostrich.GameBoy   ──▶  Ostrich.Emulation

## LR 35902 Opcodes

### Base Opcodes (Grouped)
| Opcode                             | Type    | Pseudo C                    | Description                            |
|------------------------------------|---------|-----------------------------|----------------------------------------|
| `00`                               | misc    | `;`                         | NOP                                    |
| `01,11,21,31`                      | ld16    | `rr = d16`                  | LD BC/DE/HL/SP, immediate 16-bit       |
| `02,12`                            | mem     | `[rr] = A`                  | LD (BC/DE),A                           |
| `0A,1A`                            | mem     | `A = [rr]`                  | LD A,(BC/DE)                           |
| `03,13,23,33`                      | alu16   | `rr++`                      | INC BC/DE/HL/SP                        |
| `0B,1B,2B,3B`                      | alu16   | `rr--`                      | DEC BC/DE/HL/SP                        |
| `04..3D`*                          | alu8    | `r++ / r--`                 | INC/DEC 8-bit register or `(HL)`       |
| `06,0E,16,1E,26,2E,36,3E`          | ld8     | `r = d8`                    | LD r,d8 (includes `LD (HL),d8`)        |
| `07`                               | rotate  | `A = RLC(A)`                | RLCA                                   |
| `0F`                               | rotate  | `A = RRC(A)`                | RRCA                                   |
| `17`                               | rotate  | `A = RL(A,Carry)`           | RLA                                    |
| `1F`                               | rotate  | `A = RR(A,Carry)`           | RRA                                    |
| `08`                               | mem     | `[a16] = SP`                | LD (a16),SP                            |
| `09,19,29,39`                      | alu16   | `HL += rr`                  | ADD HL,BC/DE/HL/SP                     |
| `10`                               | ctrl    | `stop`                      | STOP                                   |
| `18`                               | flow    | `PC += e8`                  | JR e8                                  |
| `20,28,30,38`                      | flow    | `if(cond) PC += e8`         | JR NZ/Z/NC/C,e8                        |
| `22,32`                            | mem     | `[HL++] = A / [HL--] = A`   | LD (HL+),A / LD (HL-),A                |
| `2A,3A`                            | mem     | `A=[HL++] / A=[HL--]`       | LD A,(HL+) / LD A,(HL-)                |
| `27`                               | alu     | `A = DAA(A)`                | DAA                                    |
| `2F`                               | alu     | `A = ~A`                    | CPL                                    |
| `37`                               | flags   | `Carry = 1`                 | SCF                                    |
| `3F`                               | flags   | `Carry = !Carry`            | CCF                                    |
| `40..7F`                           | ld8     | `r1 = r2`                   | LD r,r' (`76` is HALT)                 |
| `76`                               | ctrl    | `halt`                      | HALT                                   |
| `80..87`                           | alu     | `A += r`                    | ADD A,r                                |
| `88..8F`                           | alu     | `A += r + Carry`            | ADC A,r                                |
| `90..97`                           | alu     | `A -= r`                    | SUB A,r                                |
| `98..9F`                           | alu     | `A -= r + Carry`            | SBC A,r                                |
| `A0..A7`                           | alu     | `A &= r`                    | AND A,r                                |
| `A8..AF`                           | alu     | `A ^= r`                    | XOR A,r                                |
| `B0..B7`                           | alu     | `A \|= r`                   | OR A,r                                 |
| `B8..BF`                           | cmp     | `flags = A - r`             | CP A,r                                 |
| `C0,C8,D0,D8`                      | flow    | `if(cond) RET`              | RET NZ/Z/NC/C                          |
| `C9`                               | flow    | `RET`                       | RET                                    |
| `D9`                               | flow    | `RET; IME=1`                | RETI                                   |
| `C1,D1,E1,F1`                      | stack   | `rr = POP()`                | POP BC/DE/HL/AF                        |
| `C5,D5,E5,F5`                      | stack   | `PUSH(rr)`                  | PUSH BC/DE/HL/AF                       |
| `C2,CA,D2,DA`                      | flow    | `if(cond) PC=a16`           | JP NZ/Z/NC/C,a16                       |
| `C3`                               | flow    | `PC = a16`                  | JP a16                                 |
| `E9`                               | flow    | `PC = HL`                   | JP HL                                  |
| `C4,CC,D4,DC`                      | flow    | `if(cond) CALL a16`         | CALL NZ/Z/NC/C,a16                     |
| `CD`                               | flow    | `CALL a16`                  | CALL a16                               |
| `C6,CE`                            | alu     | `A += d8 (+Carry)`          | ADD A,d8 / ADC A,d8                    |
| `D6,DE`                            | alu     | `A -= d8 (+Carry)`          | SUB d8 / SBC A,d8                      |
| `E6,EE,F6,FE`                      | alu     | `A op d8`                   | AND/XOR/OR/CP d8                       |
| `C7,CF,D7,DF,E7,EF,F7,FF`          | flow    | `CALL vec`                  | RST 00/08/10/18/20/28/30/38            |
| `E0,F0`                            | mem     | `[FF00+a8]=A / A=[FF00+a8]` | LDH (a8),A / LDH A,(a8)                |
| `E2,F2`                            | mem     | `[FF00+C]=A / A=[FF00+C]`   | LD (C),A / LD A,(C)                    |
| `EA,FA`                            | mem     | `[a16]=A / A=[a16]`         | LD (a16),A / LD A,(a16)                |
| `E8`                               | alu16   | `SP += e8`                  | ADD SP,e8                              |
| `F8`                               | alu16   | `HL = SP + e8`              | LD HL,SP+e8                            |
| `F9`                               | ld16    | `SP = HL`                   | LD SP,HL                               |
| `F3,FB`                            | ime     | `IME = 0/1`                 | DI / EI                                |
| `D3,DB,DD,E3,E4,EB,EC,ED,F4,FC,FD` | invalid | —                           | Unused/invalid on LR35902              |

\* includes `(HL)` forms (`34`,`35`)

### CB-Prefixed Opcodes (Grouped)
| Opcode Range                       | Type    | Pseudo C                    | Description                            |
|------------------------------------|---------|-----------------------------|----------------------------------------|
| `CB 00..07`                        | rot     | `RLC r`                     | Rotate left circular                   |
| `CB 08..0F`                        | rot     | `RRC r`                     | Rotate right circular                  |
| `CB 10..17`                        | rot     | `RL r`                      | Rotate left through carry              |
| `CB 18..1F`                        | rot     | `RR r`                      | Rotate right through carry             |
| `CB 20..27`                        | shift   | `SLA r`                     | Shift left arithmetic                  |
| `CB 28..2F`                        | shift   | `SRA r`                     | Shift right arithmetic (MSB preserved) |
| `CB 30..37`                        | bitop   | `SWAP r`                    | Swap high/low nibbles                  |
| `CB 38..3F`                        | shift   | `SRL r`                     | Shift right logical                    |
| `CB 40..7F`                        | bit     | `BIT b,r`                   | Test bit `b`                           |
| `CB 80..BF`                        | bit     | `RES b,r`                   | Reset bit `b`                          |
| `CB C0..FF`                        | bit     | `SET b,r`                   | Set bit `b`                            |



