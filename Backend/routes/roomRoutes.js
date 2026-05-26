import { Router } from 'express';
import * as roomController from '../controllers/roomController.js';
import { protect } from '../middleware/auth.js';

const router = Router();

// All room routes require authentication
router.use(protect);

// POST /rooms — create room
router.post('/', roomController.create);

// GET /rooms — list active rooms
router.get('/', roomController.list);

// GET /rooms/:id — get single room
router.get('/:id', roomController.get);

// PATCH /rooms/:id/relay — set relay join code (host only, called from React after Unity allocates)
router.patch('/:id/relay', roomController.setRelay);

// POST /rooms/:id/join — join a room (password in body for private)
router.post('/:id/join', roomController.join);

// POST /rooms/:id/leave — leave a room
router.post('/:id/leave', roomController.leave);

export default router;
