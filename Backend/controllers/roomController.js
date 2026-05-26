import * as roomService from '../services/roomService.js';

// POST /rooms — create a room
export const create = async (req, res, next) => {
  try {
    const room = await roomService.createRoom(req.body, req.user);
    res.status(201).json({ success: true, room });
  } catch (error) {
    next(error);
  }
};

// GET /rooms — list active rooms
export const list = async (req, res, next) => {
  try {
    const rooms = await roomService.listRooms(req.user.id);
    res.json({ success: true, rooms });
  } catch (error) {
    next(error);
  }
};

// GET /rooms/:id — get room details
export const get = async (req, res, next) => {
  try {
    const room = await roomService.getRoom(req.params.id);
    res.json({ success: true, room });
  } catch (error) {
    next(error);
  }
};

// PATCH /rooms/:id/relay — set relay join code after Unity allocates it
export const setRelay = async (req, res, next) => {
  try {
    const room = await roomService.setRelayCode(req.params.id, req.body.relayJoinCode, req.user.id);
    res.json({ success: true, room });
  } catch (error) {
    next(error);
  }
};

// POST /rooms/:id/join — join a room
export const join = async (req, res, next) => {
  try {
    const room = await roomService.joinRoom(req.params.id, req.user.id, req.body.username, req.body.password);
    res.json({ success: true, room });
  } catch (error) {
    next(error);
  }
};

// POST /rooms/:id/leave — leave a room
export const leave = async (req, res, next) => {
  try {
    const room = await roomService.leaveRoom(req.params.id, req.user.id);
    res.json({ success: true, room });
  } catch (error) {
    next(error);
  }
};
